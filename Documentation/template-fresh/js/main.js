$(function() {

  window.onpopstate = function(event) {
    window.alert('popstate: ' + JSON.stringify(document.location));
  };

  historySupported = window.history && window.history.pushState;

  currentPage = null;

  titleEl = $('head title');
  pageTocEl = $('#page-toc');
  pageAffixEl = $('#page-affix');
  navEl = $('#navbar');
  tocEl = $('#toc');
  contentWrapperEl = $('#content-wrapper');
  affixEl = $('#affix');

  var startPagePath = $("meta[property='docfx\\:pagedata']").attr('content');
  highlightjs();
  loadAffix();
  loadPage(startPagePath);

  function loadPage(url) {
    getJSON(url, function (data) {
      updatePage(data);
    });
  }

  function updatePage(newPage) {

    var init = currentPage == null;
    var shouldLoadNav = init;
    var switchNav = !init && currentPage.navId != newPage.navId;
    var shouldLoadToc = (init || switchNav) && newPage.hasToc;

    if (!init)
      updateTitle(newPage.title);

    if (shouldLoadNav)
      loadNavBar(newPage);
    else if (switchNav)
      setNavActive(newPage.navId);

    if (!init)
      loadConceptual(newPage);

    if (shouldLoadToc)
        loadToc(newPage);
    else {
      if (!init)
        setTocActive(currentPage.path.slice(1), false);
      setTocActive(newPage.path.slice(1), true);
    }

    pageTocEl.toggleClass('hide', !newPage.hasToc);
    pageAffixEl.toggleClass('hide', !newPage.hasAffix);

    currentPage = newPage;
  }

  function updateTitle(newTitle) {
    var title = titleEl.text();
    var idx = title.indexOf('|');
    titleEl.text(newTitle + ' ' + title.slice(idx));
  }

  function loadNavBar(page) {
    $.get(page.navHtml, function (tocHtml) {
      navEl.html(tocHtml);
      setNavActive(page.navId);
      hookNavEvents();
    });
  }

  function loadToc(page) {
    $.get(page.tocHtml, function (tocHtml) {
      tocEl.html(tocHtml);
      setTocActive(page.path.slice(1), true);
      hookTocEvents();
    });
  }

  function loadConceptual(page) {
    var conceptualPath = page.filename + '.html.partial';
    $.get(conceptualPath, function (contentHtml) {
      contentWrapperEl.html(contentHtml);
      highlightjs();
      if (page.hasAffix)
        loadAffix();
    });
  }

  function highlightjs() {
    contentWrapperEl.find('pre code').each(function(i, block) {
      hljs.highlightBlock(block);
    });
  }

  function loadAffix() {
    var hierarchy = getHeadingHierarchy();
    var html = buildAffix(hierarchy, 1);
    affixEl.html(html);
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


  function setNavActive(name) {
    navEl.find('li').each(function (i, e) {
      var obj = $(e);
      if (name === obj.attr('navname'))
        obj.addClass('active');
      else
        obj.removeClass('active');
    });
  }

  function setTocActive(elements, value) {
    var lis = getTocLis(elements);
    if (value) {
      for (var i = 0; i < lis.length; i++)
        lis[i].addClass('active');
    } else {
      for (var i = 0; i < lis.length; i++)
        lis[i].removeClass('active');
    }
  }

  function getTocLis(elements) {
    var ul = tocEl.children()
    if (ul.length == 0)
      return [];
    // find the leaf element
    for (var i = 0; i < elements.length; i++) {
      var el = elements[i];
      var li = ul.children('[filename=' + el + ']');
      // if it's not a direct child, look through all descendants
      if (li.length == 0)
        li = ul.find('[filename=' + el + ']');
      if (li.length == 0)
        break;

      ul = li.children('ul');
      if (ul.length == 0)
        break;
    }

    if (!li || li.length == 0)
      return [];

    // walk up the toc and grab all li's
    lis = [];
    while (li && li.attr('id') != 'toc') {
      if (li.is('li'))
        lis.push(li);
      li = li.parent();
    }

    return lis;
  }

  function hookNavEvents() {
    makeLocalLinksDynamic(navEl);
  }

  function hookTocEvents() {
    makeLocalLinksDynamic(tocEl);
  }

  function makeLocalLinksDynamic(element) {
    if (historySupported) {
      element.find('a').click(function(e) {
        var a = $(e.target);
        var href = a.attr('href');
        if (isRelativePath(href) && href.endsWith('.html')) {
          e.preventDefault();
          var dataPath = href.substring(0, href.lastIndexOf('.')) + '.json';
          loadPage(dataPath);

          history.pushState({href: href}, null, href);
        }
      });
    }
  }

  function getJSON(url, success) {
    $.ajax({
      dataType: 'json',
      url: url,
      success: success,
      mimeType: 'application/json'
    });
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

  function isRelativePath(path) {
    return !isAbsolutePath(path);
  }

  function isAbsolutePath(path) {
      return /^(\w+:)?\/\//g.test(path);
  }
});

