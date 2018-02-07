var common = require('./common.js');

exports.transform = function (model) {

  page = {};

  page.title = model.title
  page.path = '/' + common.stripAllExtensions(model._path);
  var navPathIndex = model._path.indexOf('/');
  page.navId = model._path.substring(0, navPathIndex) || model._path.substring(0, model._path.indexOf('.'));
  page.nav = common.stripExtension(model._navRel);
  page.toc = common.stripExtension(model._tocRel);
  page.hasToc = !model.disableToc && model._navRel != model._tocRel;
  page.hasBreadcrumb = !model._disableBreadcrumb;
  page.hasContributionLink = !model._disableContribution;
  if (page.hasContributionLink)
    page.contributionLink = model.docurl || common.getImproveTheDocHref(model, model._gitContribute, model._gitUrlPattern);
  page.hasAffix = !model._disableAffix;

  model.page = JSON.stringify(page);
 
  return model;

  function parsePagePath(path) {
    // hierarchical location of the page e.g. ["manual","getting_started","setting_up"]
    // filter(String) filters empty entries
    return path.split("/").filter(String).map(common.stripAllExtensions);
  }
};

