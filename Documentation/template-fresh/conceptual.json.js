var common = require('./common.js');

exports.transform = function (model) {

  page = {};

  page.title = model.title
  page.filename = '/' + common.stripAllExtensions(model._path);
  page.path = parsePagePath(model._path);
  page.navId = common.getNavId(model._path);
  page.navHtml = model._navRel;
  page.tocHtml = model._tocRel;
  page.hasToc = !model.disableToc && model._navRel != model._tocRel;
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

