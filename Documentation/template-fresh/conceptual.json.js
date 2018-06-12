var common = require('./common.js');

exports.transform = function (model) {

  var page = {};

  page.title = model.title
  page.path = '/' + common.stripAllExtensions(model._path);
  page.navIndex = findNavIndex();
  page.nav = common.stripExtension(model._navRel);
  page.toc = model._tocRel === model._navRel ? null : common.stripExtension(model._tocRel);
  page.tocIndex = findTocIndex();
  page.hasToc = !model.disableToc && model._navRel != model._tocRel;
  page.hasBreadcrumb = !model._disableBreadcrumb;
  page.hasContributionLink = !model._disableContribution;
  page.autoExpandToc = !model._disableAutoExpandToc;
  if (page.hasContributionLink)
    page.contributionLink = model.docurl || common.getImproveTheDocHref(model, model._gitContribute, model._gitUrlPattern);
  page.hasAffix = !model._disableAffix;
  page.affixSubLevel = !model.disableAffixSubLevel;
  page.hasPrevnext = !model._disablePrevnext;

  model.page = JSON.stringify(page);
 
  return model;

  function findNavIndex() {
    var nav = model.__global._shared[model._navKey];
    if (!nav || !nav.items)
      return -1;

    // find the nav item with tocHref equal to _tocPath
    var index = findNodeIndex(nav.items, function (item) { 
      return item.tocHref === model._tocPath || item.topicHref === model._path; 
    });

    return index;
  }

  function findTocIndex() {
    if (model._tocRel === model._navRel)
      return -1;
    var toc = model.__global._shared[model._tocKey];
    if (!toc || !toc.items)
      return -1;

    return findNodeIndex(toc.items, function (item) { return model._path.indexOf(item.topicHref) >= 0; });
  }

  function findNodeIndex(items, filter) {
    return findNodeIndexRec(items, filter, {index: 0});

    function findNodeIndexRec(items, filter, indexBox) {
      for (var i = 0; i < items.length; i++) {
        var item = items[i];

        if (filter(item))
          return indexBox.index;

        indexBox.index += 1;

        if (item.items) {
          // if we found the index, bubble it up
          var result = findNodeIndexRec(item.items, filter, indexBox);
          if (result >= 0)
            return result;
        }
      }

      return -1;
    }
  }
};

