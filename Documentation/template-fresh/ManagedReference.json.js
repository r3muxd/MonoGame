var mrefCommon = require('./ManagedReference.common.js');
var pageCommon = require('./page.common.js');

exports.transform = function (model) {
  model.title = mrefCommon.getTitle(model);
  model.page = pageCommon.createPage(model);
  return model;
};

