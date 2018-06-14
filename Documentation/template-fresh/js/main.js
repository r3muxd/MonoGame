"use strict";
$(function() {

  window.onpopstate = function(event) {
    saveScrollPos();
    var scrollPos = event.state ? event.state.scrollPos : null;
    loadPage(stripExtension(window.location.pathname), scrollPos, window.location.hash.substring(1));
  };
  window.onbeforeunload = function(event) {
    saveScrollPos();
  };

  var historySupported = window.history && window.history.pushState;

  var currentPage = null;
  var nav = null;
  var toc = null;
  var activeHeadingIndex = -1;

  // cached DOM elements (as jQuery objects)
  var titleEl = $('head title');
  var pageTocEl = $('#page-toc');
  var pageAffixEl = $('#page-affix');
  var navEl = $('#navbar');
  var filterEl = pageTocEl.find('#toc-filter-input');
  var filterClearEl = pageTocEl.find('#filter-clear');
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

  var baseDir = $("meta[property='docfx\\:baseDir']").attr('content');
  var basePath = baseDir ? '/' + baseDir : '';

  init();
  
  function init() {

    $('#toc-toggle').click(function (ev) {
      pageTocEl.toggleClass('shown');
      ev.stopPropagation();
    });

    var mainPage = $('#page-main');
    mainPage.click(hideTocForMobile);

    pageTocEl.addClass('no-transition');
    hideTocForMobile();
    // we need to force the browser to reload the element before the no-transition class is removed
    pageTocEl.hide().show(0);
    pageTocEl.removeClass('no-transition');

    var touchStart = {startLeft: false, x: 0, y: 0};
    pageScrollEl.on('touchstart', function (ev) {
      var t = ev.targetTouches[0];
      if (t.clientX < 30)
        touchStart = { startLeft: true, x: t.clientX, y: t.clientY };
    });
    pageScrollEl.on('touchmove', function (ev) {
      if (!touchStart.startLeft || pageTocEl.hasClass('shown'))
        return;
      var t = ev.targetTouches[0];
      if (t.clientX - touchStart.x > 50) {
        pageTocEl.addClass('shown');
        touchStart = {startLeft: false, x: 0, y: 0};
      }
    });
    pageScrollEl.on('touchend', function (ev) {
      touchStart = {startLeft: false, x: 0, y: 0};
    });


    var startPagePath = $("meta[property='docfx\\:pagedata']").attr('content');
    var scrollPos = null;
    if (historySupported && history.state && history.state.scrollPos)
      scrollPos = history.state.scrollPos;
    loadPage(startPagePath, scrollPos, window.location.hash);
  }

  function hideTocForMobile() {
    if (isReallySmall())
      pageTocEl.removeClass('shown');
  }
 
  function loadPage(path, scrollPos, hash) {
    // don't reload page
    if (currentPage && currentPage.path === path) {
      setScroll(scrollPos, hash);
      return false;
    }

    getJSON(path + '.json', function (page) {
      updatePage(page, scrollPos, hash);
      hideTocForMobile();
    }, function () {
      console.error('Failed to load page: ' + path);
      // reload the page on failure to go to 404
      //window.location.reload();
    });

    return true;
  }

  function updatePage(newPage, scrollPos, hash) {

    var init = currentPage == null;
    var shouldLoadNav = init;
    var switchNav = !init && currentPage.navIndex !== newPage.navIndex;
    var shouldLoadToc = (init || switchNav) && newPage.toc !== null;

    if (!init)
      updateTitle(newPage.title);

    if (shouldLoadNav)
      loadNavBar(currentPage, newPage);
    else if (switchNav)
      loadAfterNav(currentPage, newPage);

    loadConceptual(newPage, scrollPos, hash);

    if (shouldLoadToc)
      loadToc(currentPage, newPage);
    else
      loadAfterToc(currentPage, newPage, currentPage && newPage.toc !== null);

    clearTocFilter();

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
      var tocHtml = buildTreeHtml(toc, createTocEntry, true);
      tocEl.html(tocHtml);
      hookTocEvents();

      loadAfterToc(oldPage, newPage, false);
    });
  }

  function createTocEntry(node, tree) {
    return createAnchorJquery(node);
  }

  function buildTreeHtml(tree, createEntry, expander = false) {
    var root = buildTreeRec(tree.rootNodes(), 1, expander);
    return root;

    function buildTreeRec(nodes, level, expander) {
      var ul = $('<ul>').addClass('nav level' + level);
      if (expander)
        ul.addClass('collapsed');
      for (var i = 0; i < nodes.length; i++) {
        var node = nodes[i];
        var children = tree.children(node.index);

        var li = $('<li class="pos-rel">');
        if (children.length > 0 && expander) {
          var expander = $('<span class="expander" />');
          expander.click(function() {
            toggleExpander($(this));
          });
          li.append(expander);
        }
        li.append(createEntry(node, tree));

        node.element = li;

        if (children.length > 0) {
          var subUl = buildTreeRec(children, level + 1, expander);
          li.append(subUl);
        }
        ul.append(li);
      }

      return ul;
    }
  }

  function toggleExpandLi(e, value) {
    var expander = e.children('span.expander');
    if (expander)
      toggleExpander(expander, value);
  }

  function toggleExpander(e, value) {
    e.toggleClass('expanded', value);
    var expanded = e.hasClass('expanded');
    e.nextAll('ul').toggleClass('collapsed', !expanded);
  }

  function loadAfterNav(oldPage, newPage) {
    if (oldPage && oldPage.navIndex >= 0)
      nav.nodes[oldPage.navIndex].element.removeClass('active');
    if (newPage.navIndex >= 0)
      nav.nodes[newPage.navIndex].element.addClass('active');
  }

  function loadAfterToc(oldPage, newPage, keptToc) {
    if (keptToc) {
      if (oldPage.tocIndex >= 0)
        toggleTocActive(oldPage, false);
      // fold in all elements
      toc.doAll(n => toggleExpandLi(n.element, false));
    }

    if (newPage.tocIndex >= 0) {
      toggleTocActive(newPage, true);
      if (newPage.autoExpandToc) {
        // expand active elements
        toc.doSelf(newPage.tocIndex, n => toggleExpandLi(n.element, true));
        toc.doAncestors(newPage.tocIndex, n => toggleExpandLi(n.element, true));
      }
    }

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
    var html = '<span class="semibold nowrap">' + node.name + '</span>';

    var index = node.parent;
    while (index != null) {
      node = toc.nodes[index];
      html = createAnchorHtml(node, 'nowrap') + ' <span class="mg-icons">&#xe802;</span> ' + html;
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
      prevEl.attr('href', basePath + prev.path + '.html');
    else
      prevEl.removeAttr('href');
    var next = toc.next(page.tocIndex);
    if (next)
      nextEl.attr('href', basePath + next.path + '.html');
    else
      nextEl.removeAttr('href');

    makeLocalLinksDynamic(prevnextEl);
  }

  function loadConceptual(page, scrollPos, hash) {
    var conceptualPath = basePath + page.path + '.html.partial';
    $.get(conceptualPath, function (contentHtml) {
      contentWrapperEl.html(contentHtml);
      loadAfterConceptual(page, scrollPos, hash);
   });
  }

  function loadAfterConceptual(page, scrollPos, hash) {
    highlightjs();

    // unhook possible scroll event handlers
    pageScrollEl.off('scroll');

    if (page.hasAffix)
      loadAffix(page);
    else
      pageAffixEl.toggleClass('hide', true);

    setScroll(scrollPos, hash);

    if (page.subtitleAnchors);
      addSubtitleAnchors();

    makeLocalLinksDynamic(contentWrapperEl);
  }

  function highlightjs() {
    contentWrapperEl.find('pre code').each(function(i, block) {
      hljs.highlightBlock(block);
    });
  }

  function addSubtitleAnchors() {
    contentWrapperEl.find('h2, h3, h4').each(function(i) {
      console.log('Anchoring title: ' + $(this).text());
      var id = $(this).attr('id');
      var anchor = $('<a class="subtitle-anchor mg-icons hide" href="#' + id + '">&#xF07B;</a>');
      $(this).append(anchor);

      $(this).mouseenter(function () {
        anchor.removeClass('hide');
      });
      $(this).mouseleave(function () {
        anchor.addClass('hide');
      });
    });
  }

  function loadAffix(page) {
    var headingTree = getHeadingTree(page.affixSubLevel);
    if (headingTree.size() === 0) {
      pageAffixEl.addClass('hide');
    } else {
      var html = buildTreeHtml(headingTree, createAnchorJquery);
      affixEl.html(html);
      makeLocalLinksDynamic(affixEl);
      pageAffixEl.removeClass('hide');

      pageScrollEl.scroll(function () {
        scrollAffix(headingTree);
      });

      scrollAffix(headingTree);
    }
  }

  function getHeadingTree(subLevel) {
    var article = contentWrapperEl.find('article');

    // the root heading level is the first level with more than 1 heading
    // we go down at most 1 level from the root level and no further than h4
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

    var findStr = rootHeading;
    if (subLevel)
      findStr += ',' + subHeading;

    var headings = article.find(findStr);

    var treeBuilder = new TreeBuilder();
    for (var i = 0; i < headings.length; i++) {
      var heading = headings[i];
      // get the heading number from the tag name (nodeName) and subtract root level for the depth
      var depth = Number(heading.nodeName[1]) - rootHeadingLevel;
      var myHeading = { name: htmlEncode(heading.textContent), href: '#' + heading.id, headingElement: heading };
      treeBuilder.push(myHeading, depth);
    }

    return treeBuilder.finish();
  }

  function scrollAffix(tree) {
    var scrollTop = pageScrollEl.scrollTop();

    for (var i = tree.nodes.length - 1; i >= 0; i--) {
      if (tree.nodes[i].headingElement.offsetTop < scrollTop)
        break;
    }

    if (activeHeadingIndex === i)
      return;

    if (activeHeadingIndex != -1) {
      tree.doSelf(activeHeadingIndex, n => n.element.removeClass('active'));
      tree.doAncestors(activeHeadingIndex, n => n.element.removeClass('active'));
    }

    activeHeadingIndex = i;

    var newTop = .2 * pageAffixEl.height();
    if (activeHeadingIndex !== -1) {
      tree.doSelf(activeHeadingIndex, n => n.element.addClass('active'));
      tree.doAncestors(activeHeadingIndex, n => n.element.addClass('active'));

      var rootHeadingNode = tree.rootParent(activeHeadingIndex);
      newTop -= rootHeadingNode.element.position().top;
    }

    affixEl.css('top', newTop);
  }

  function hookNavEvents() {
    makeLocalLinksDynamic(navEl);
  }

  function hookTocEvents() {
    hookTocFilterEvent();
    makeLocalLinksDynamic(tocEl);
  }

  function hookTocFilterEvent() {
    filterClearEl.click(clearTocFilter);
    filterEl.off('input');
    filterEl.on('input', function (e) {
      var text = e.currentTarget.value.trim();
      if (!text) {
        filterClearEl.addClass('hide');
        toc.doAll(n => n.element.removeClass('hide search-result direct-search-result'));
        filterNoResultsEl.addClass('hide');
      } else {
        filterClearEl.removeClass('hide');
        toc.doAll(n => n.element.addClass('hide').removeClass('search-result direct-search-result'));
        var match = 0;
        for (var i = 0; i < toc.nodes.length; i++) {
          var node = toc.nodes[i];
          if (node.name.toLowerCase().indexOf(text.toLowerCase()) > -1) {
            toc.doSelf(i, n => n.element.removeClass('hide').addClass('search-result direct-search-result'));
            toc.doAncestors(i, n => n.element.removeClass('hide').addClass('search-result'));
            i = toc.doBelow(i, n => n.element.removeClass('hide'));
            match++;
          }
        }
        filterNoResultsEl.toggleClass('hide', match > 0);
      }
    });
  }

  function makeLocalLinksDynamic(element) {
    if (historySupported) {
      var as = element.find('a[href]');
      as.off('click');
      for (var i = 0; i < as.length; i++) {
        var a = as[i];
        var path = a.pathname;
        if (path && isRelativePath(path) && path.endsWith('.html')) {
          a.onclick = function(e) {
            var a = e.currentTarget;
            e.preventDefault();
            var dataPath = stripExtension(a.pathname);
            saveScrollPos();
            if (loadPage(dataPath, null, a.hash.substring(1)))
              history.pushState(null, null, a.href);
          };
        }
      }
    }
  }

  function saveScrollPos() {
    var state = {scrollPos: pageScrollEl.scrollTop()};
    history.replaceState(state, null, null);
  }

  function setScroll(scrollPos, hash) {
    if (scrollPos != null)
      pageScrollEl.scrollTop(scrollPos);
    else if (hash)
      window.location.hash = hash;
    else
      pageScrollEl.scrollTop(0);
  }

  function clearTocFilter() {
    if (filterEl[0].value) {
      filterEl[0].value = '';
      filterEl.trigger('input');
    }
  }

  function createAnchorJquery(node) {
    return $(createAnchorHtml(node));
  }

  function createAnchorHtml(node, cls = '') {
    var href = node.href || (basePath + node.path + '.html');
    return '<a href="' + href + '" index="' +  node.index + '" class="' + cls + '">' + node.name + '</a>';
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

  function isReallySmall() {
    return window.matchMedia("(max-width: 414px)").matches;
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

