"use strict";
$(function() {

  window.onpopstate = function(event) {
    loadPage(stripExtension(document.location.pathname));
  };

  var historySupported = window.history && window.history.pushState;

  var currentPage = null;
  var nav = null;
  var toc = null;

  // cached DOM elements (as jQuery objects)
  var titleEl = $('head title');
  var pageTocEl = $('#page-toc');
  var pageAffixEl = $('#page-affix');
  var navEl = $('#navbar');
  var filterEl = pageTocEl.find('#toc-filter-input');
  var filterNoResultsEl = pageTocEl.find('#toc-no-results');
  var tocEl = pageTocEl.find('#toc');
  var pageScrollEl = $('#page-scroll');
  var tocToggleWrapperEl = pageScrollEl.find('#toc-toggle-wrapper');
  var contentWrapperEl = $('#content-wrapper');
  var breadcrumbWrapperEl = $('#breadcrumb-wrapper');
  var breadcrumbEl = $('#breadcrumb');
  var contributionLinkEl = $('#contribution-link');
  var prevnextEl = $('#prevnext');
  var prevEl = prevnextEl.find('#prev');
  var nextEl = prevnextEl.find('#next');
  var affixEl = $('#affix');

  init();
  
  function init() {
    $('#toc-toggle').click(function () {
      pageTocEl.toggleClass('shown');
    });
    var startPagePath = $("meta[property='docfx\\:pagedata']").attr('content');
    loadPage(startPagePath);
  }

  function loadPage(path) {
    // don't reload page
    if (currentPage && currentPage.path === path)
      return false;

    getJSON(path + '.json', function (page) {
      page.navIndex = -1;
      page.tocIndex = -1;
      updatePage(page);
      pageScrollEl.scrollTop(0);

      if (filterEl.value) {
        filterEl[0].value = '';
        filterEl.trigger('input');
      }
    }, function () {
      // reload the page on failure to go to 404
      location.reload();
    });

    return true;
  }

  function updatePage(newPage) {

    var init = currentPage == null;
    var shouldLoadNav = init;
    var switchNav = !init && currentPage.navId != newPage.navId;
    var shouldLoadToc = (init || switchNav);

    if (!init)
      updateTitle(newPage.title);

    if (shouldLoadNav)
      loadNavBar(currentPage, newPage);
    else if (switchNav)
      loadAfterNav(currentPage, newPage);
    else
      newPage.navIndex = currentPage.navIndex;

    loadConceptual(newPage);

    if (shouldLoadToc)
      loadToc(currentPage, newPage);
    else
      loadAfterToc(currentPage, newPage, false);

    pageTocEl.toggleClass('hide', !newPage.hasToc);
    tocToggleWrapperEl.toggleClass('hide', !newPage.hasToc);
    contributionLinkEl.toggleClass('hide', !newPage.hasContributionLink);

    if (newPage.hasContributionLink)
      contributionLinkEl.attr('href', newPage.contributionLink);

    currentPage = newPage;

    console.log('Loaded Page:');
    console.log(JSON.stringify(currentPage, null, 4));
  }

  function updateTitle(newTitle) {
    var title = titleEl.text();
    var idx = title.indexOf('|');
    titleEl.text(newTitle + ' ' + title.slice(idx));
  }

  function loadNavBar(oldPage, newPage) {
    getJSON(newPage.nav + '.json', function (navNodes) {
      nav = new Tree(navNodes);
      var navHtml = buildTreeHtml(nav, createAnchorJquery);
      navEl.html(navHtml);
      loadAfterNav(oldPage, newPage);
      hookNavEvents();
    });
  }

  function loadToc(oldPage, newPage) {
    getJSON(newPage.toc + '.json', function (tocNodes) {
      toc = new Tree(tocNodes);
      var tocHtml = buildTreeHtml(toc, createTocEntry);
      tocEl.html(tocHtml);
      hookTocEvents();

      loadAfterToc(oldPage, newPage, true);
    });
  }

  function createTocEntry(node, tree) {
    var anchor = createAnchorJquery(node);
    //if (tree.hasChildren(node.index))
    //  $(anchor).addClass('expander');

    return anchor;
  }

  function buildTreeHtml(tree, createEntry) {
    var root = buildTreeRec(tree.rootNodes(), 1);
    return root;

    function buildTreeRec(nodes, level) {
      var ul = $('<ul>').addClass('nav level' + level);
      for (var i = 0; i < nodes.length; i++) {
        var node = nodes[i];
        var li = $('<li class="pos-rel">');
        li.append(createEntry(node, tree));

        node.element = li;

        var children = tree.children(node.index);
        if (children.length > 0) {
          var subUl = buildTreeRec(children, level + 1);
          li.append(subUl);
        }
        ul.append(li);
      }

      return ul;
    }
  }

  function loadAfterNav(oldPage, newPage) {
    for (var i = 0; i < nav.nodes.length; i++) {
      if (!nav.nodes[i].path)
        continue;
      var nnp = nav.nodes[i].path.slice(1);
      var nodeNavId = nnp.substring(0, nnp.indexOf('/')) || nnp;
      if (nodeNavId === newPage.navId) {
        newPage.navIndex = i;
        break;
      }
    }

    if (oldPage && oldPage.navIndex >= 0)
      nav.nodes[oldPage.navIndex].element.removeClass('active');
    if (newPage.navIndex >= 0)
      nav.nodes[newPage.navIndex].element.addClass('active');
  }

  function loadAfterToc(oldPage, newPage, tocChanged) {
    for (var i = 0; i < toc.nodes.length; i++) {
      if (toc.nodes[i].path === newPage.path)
        newPage.tocIndex = i;
    }

    if (!tocChanged)
      toggleTocActive(oldPage, false);
    toggleTocActive(newPage, true);

    if (newPage.tocIndex < 0) {
      newPage.hasBreadcrumb = false;
      newPage.hasPrevnext = false;
    }

    updateBreadcrumb(newPage);
    updatePrevnext(newPage);
  }

  function toggleTocActive(page, value) {
    if (!page || page.tocIndex < 0)
      return;

    toc.doSelf(page.tocIndex, n => n.element.toggleClass('active', value));
    toc.doAncestors(page.tocIndex, n => n.element.toggleClass('active', value));
  }

  function updateBreadcrumb(page) {

    breadcrumbWrapperEl.toggleClass('hide', !page.hasBreadcrumb);
    if (!page.hasBreadcrumb)
      return;

    var node = toc.nodes[page.tocIndex];
    var html = '<span class="semibold">' + node.name + '</span>';

    var index = node.parent;
    while (index != null) {
      node = toc.nodes[index];
      html = createAnchorHtml(node) + ' <span class="mg-icons">&#xe802;</span> ' + html;
      index = node.parent;
    }

    html = '<div>' + html + '</div>';

    breadcrumbEl.html(html);
    makeLocalLinksDynamic(breadcrumbEl);
  }

  function updatePrevnext(page) {

    prevnextEl.toggleClass('hide', !page.hasPrevnext);
    if (!page.hasPrevnext)
      return;

    var prev = toc.prev(page.tocIndex);
    if (prev)
      prevEl.attr('href', prev.path + '.html');
    else
      prevEl.removeAttr('href');
    var next = toc.next(page.tocIndex);
    if (next)
      nextEl.attr('href', next.path + '.html');
    else
      nextEl.removeAttr('href');

    makeLocalLinksDynamic(prevnextEl);
  }

  function loadConceptual(page) {
    var conceptualPath = page.path + '.html.partial';
    $.get(conceptualPath, function (contentHtml) {
      contentWrapperEl.html(contentHtml);
      loadAfterConceptual(page);
   });
  }

  function loadAfterConceptual(page) {
    highlightjs();

    // unhook possible scroll event handlers
    pageScrollEl.off('scroll');

    if (page.hasAffix)
      loadAffix();
    else
      pageAffixEl.toggleClass('hide', true);

    makeLocalLinksDynamic(contentWrapperEl);
  }

  function highlightjs() {
    contentWrapperEl.find('pre code').each(function(i, block) {
      hljs.highlightBlock(block);
    });
  }

  function loadAffix() {
    var headingTree = getHeadingTree();
    //console.log('Heading tree:');
    //console.log('  ' + JSON.stringify(headingTree));
    if (headingTree.size() === 0) {
      pageAffixEl.addClass('hide');
    } else {
      var html = buildTreeHtml(headingTree, createAnchorJquery);
      affixEl.html(html);
      pageAffixEl.removeClass('hide');

      pageScrollEl.scroll(function () {
        scrollAffix();
      });
    }
  }

  function getHeadingTree() {
    var article = contentWrapperEl.find('article');

    // the root heading level is the first level with more than 1 heading
    // we go down at most 1 level from the root level and no further than h4
    // Basically it turns this: [h1, h2, h2, h3, h4, h3]
    // Into this:               [h2, h2[h3, h3]]
    var deepest = 4;

    // first level with more than 1 element
    for (var rootHeadingLevel = 1; rootHeadingLevel < deepest; rootHeadingLevel++) {
      if (article.find('h' + rootHeadingLevel).length > 1)
        break;
    }

    if (rootHeadingLevel == deepest)
      return new Tree([]);

    var rootHeading = 'h' + rootHeadingLevel;
    var subHeading = 'h' + (rootHeadingLevel + 1);
    var headings = article.find(rootHeading + ',' + subHeading);

    var treeBuilder = new TreeBuilder();
    for (var i = 0; i < headings.length; i++) {
      var heading = headings[i];
      var depth = Number(heading.nodeName[1]) - rootHeadingLevel;
      var myHeading = { name: htmlEncode(heading.textContent), href: '#' + heading.id, element: $(heading) };
      treeBuilder.push(myHeading, depth);
    }

    return treeBuilder.finish();
  }

  function scrollAffix() {

  }

  function hookNavEvents() {
    makeLocalLinksDynamic(navEl);
  }

  function hookTocEvents() {
    hookTocFilterEvent();
    makeLocalLinksDynamic(tocEl);
  }

  function hookTocFilterEvent() {
    filterEl.off('input');
    filterEl.on('input', function (e) {
      var text = e.currentTarget.value.trim();
      if (!text) {
        toc.doAll(n => n.element.removeClass('hide'));
        filterNoResultsEl.addClass('hide');
      } else {
        var match = 0;
        for (var i = 0; i < toc.nodes.length; i++) {
          var node = toc.nodes[i];
          if (node.name.toLowerCase().indexOf(text.toLowerCase()) > -1) {
            toc.doSelf(i, n => n.element.removeClass('hide'));
            toc.doAncestors(i, n => n.element.removeClass('hide'));
            i = toc.doBelow(i, n => n.element.removeClass('hide'));
            match++;
          } else {
            node.element.addClass('hide');
          }
          filterNoResultsEl.toggleClass('hide', match > 0);
        }
      }
    });
  }

  function makeLocalLinksDynamic(element) {
    if (historySupported) {
      element.find('a[href]').off('click').click(function(e) {
        var a = $(e.currentTarget);
        var href = a.attr('href');
        if (href && isRelativePath(href) && href.endsWith('.html')) {
          e.preventDefault();
          var dataPath = stripExtension(href);
          if (loadPage(dataPath))
            history.pushState({href: href}, null, href);
        }
      });
    }
  }

  function createAnchorJquery(node) {
    return $(createAnchorHtml(node));
  }

  function createAnchorHtml(node) {
    var href = node.path + '.html';
    return '<a href="' + href + '" index="' +  node.index + '">' + node.name + '</a>';
  }

  function getJSON(url, success, failure) {
    $.ajax({
      dataType: 'json',
      url: url,
      mimeType: 'application/json'
    })
      .done(success)
      .fail(failure);
  }

  function htmlEncode(str) {
    if (!str) return str;
    return str
      .replace(/&/g, '&amp;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');
  }

  function stripExtension(path) {
    return path.substring(0, path.lastIndexOf('.'));
  }

  function isRelativePath(path) {
    return !isAbsolutePath(path);
  }

  function isAbsolutePath(path) {
      return /^(\w+:)?\/\//g.test(path);
  }
});

