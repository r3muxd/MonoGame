var pageCommon = require('./page.common.js');

exports.transform = function (model) {
  model.page = pageCommon.createPage(model);
  return model;
};

