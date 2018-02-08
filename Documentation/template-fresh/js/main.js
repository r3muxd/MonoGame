$(function() {

  window.onpopstate = function(event) {
    loadPage(stripExtension(document.location.pathname));
  };

  historySupported = window.history && window.history.pushState;

  currentPage = null;
  nav = null;
  toc = null;

  // cached DOM elements (as jQuery objects)
  titleEl = $('head title');
  pageTocEl = $('#page-toc');
  pageAffixEl = $('#page-affix');
  navEl = $('#navbar');
  filterEl = $('#toc-filter-input');
  tocEl = $('#toc');
  contentWrapperEl = $('#content-wrapper');
  breadcrumbWrapperEl = $('#breadcrumb-wrapper');
  breadcrumbEl = $('#breadcrumb');
  contributionLinkEl = $('#contribution-link');
  prevEl = contentWrapperEl.find('#prev');
  nextEl = contentWrapperEl.find('#next');
  affixEl = $('#affix');

  init();
  
  function init() {
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
      loadAfterToc(currentPage, newPage);

    pageTocEl.toggleClass('hide', !newPage.hasToc);
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
      var navHtml = buildTocHtml(nav);
      navEl.html(navHtml);
      loadAfterNav(oldPage, newPage);
      hookNavEvents();
    });
  }

  function loadToc(oldPage, newPage) {
    getJSON(newPage.toc + '.json', function (tocNodes) {
      toc = new Tree(tocNodes);
      var tocHtml = buildTocHtml(toc);
      tocEl.html(tocHtml);
      hookTocEvents();

      loadAfterToc(oldPage, newPage);
    });
  }

  function buildTocHtml(tree) {
    var root = buildTocRec(tree, tree.rootNodes(), 1);
    return root;

    function buildTocRec(tree, nodes, level) {
      var ul = $('<ul>').addClass('nav level' + level);
      for (var i = 0; i < nodes.length; i++) {
        var node = nodes[i];
        var li = $('<li>');
        li.append($(createAnchorHtml(node)));

        node.liElement = li;

        if (!node.leaf) {
          var subUl = buildTocRec(tree, tree.children(node.index), level + 1);
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
      nav.nodes[oldPage.navIndex].liElement.removeClass('active');
    if (newPage.navIndex >= 0)
      nav.nodes[newPage.navIndex].liElement.addClass('active');
  }

  function loadAfterToc(oldPage, newPage) {
    for (var i = 0; i < toc.nodes.length; i++) {
      if (toc.nodes[i].path === newPage.path)
        newPage.tocIndex = i;
    }

    toggleTocActive(oldPage, false);
    toggleTocActive(newPage, true);


    if (newPage.tocIndex < 0)
      newPage.hasBreadcrumb = false;

    breadcrumbWrapperEl.toggleClass('hide', !newPage.hasBreadcrumb);

    if (newPage.hasBreadcrumb)
      updateBreadcrumb(newPage);
  }

  function toggleTocActive(page, value) {
    if (!page || page.tocIndex < 0)
      return;

    toc.doSelf(page.tocIndex, n => n.liElement.toggleClass('active', value));
    toc.doAncestors(page.tocIndex, n => n.liElement.toggleClass('active', value));
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

  function updateBreadcrumb(page){
    var node = toc.nodes[page.tocIndex];
    html = '<span class="semibold">' + node.name + '</span>';

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

  function loadAffix() {
    var hierarchy = getHeadingHierarchy();
    if (hierarchy.length == 0) {
      pageAffixEl.addClass('hide');
    } else {
      var html = buildAffix(hierarchy, 1);
      affixEl.html(html);
      pageAffixEl.removeClass('hide');
    }
  }

  function getHeadingHierarchy() {
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
      return [];

    var rootHeading = 'h' + rootHeadingLevel;
    var subHeading = 'h' + (rootHeadingLevel + 1);
    var headings = article.find(rootHeading + ',' + subHeading);

    var hierarchy = [];
    for (var i = 0; i < headings.length; i++) {
      var heading = $(headings[i]);
      var id = heading.attr('id');
      if (!id)
        continue;
      var myHeading = { name: htmlEncode(heading.text()), href: '#' + id, items: [] };

      if (heading.is(rootHeading)) {
        hierarchy.push(myHeading);
      } else if (heading.is(subHeading)) {
        if (hierarchy.length != 0)
            hierarchy[hierarchy.length -1].items.push(myHeading);
      }
    }

    return hierarchy;
  }

  function buildAffix(items, level) {
    if (!items || items.length == 0)
      return '';
    var html = '';
    html += '<ul class="nav level' + level + '">';
    for (var i = 0; i < items.length; i++) {
      var item = items[i];
      html += '<li>';
      html += '<a href="' + item.href + '">' + item.name + '</a>';
      html += buildAffix(item.items, level + 1);
      html += '</li>';
    }
    html  += '</ul>';
    return html;
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
      var text = e.target.value;
      if (!text) {
        toc.doAll(n => n.liElement.removeClass('hide'));
      } else {
        for (var i = 0; i < toc.nodes.length; i++) {
          var node = toc.nodes[i];
          if (node.name.toLowerCase().indexOf(text.toLowerCase()) > -1) {
            toc.doSelf(i, n => n.liElement.removeClass('hide'));
            toc.doAncestors(i, n => n.liElement.removeClass('hide'));
            toc.doBelow(i, n => n.liElement.removeClass('hide'));
          } else {
            node.liElement.addClass('hide');
          }
        }
      }
    });
  }

  function makeLocalLinksDynamic(element) {
    if (historySupported) {
      element.find('a').click(function(e) {
        var a = $(e.target);
        var href = a.attr('href');
        if (isRelativePath(href) && href.endsWith('.html')) {
          e.preventDefault();
          var dataPath = stripExtension(href);
          if (loadPage(dataPath))
            history.pushState({href: href}, null, href);
        }
      });
    }
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

