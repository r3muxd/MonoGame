
var common = require("./common.js");

exports.transform = function (model) {
  model.dataPath = common.getFileNameWithoutAnyExtensions(model._key);
  model.hasToc = !model.disableToc && model._navRel != model._tocRel;
  model.docurl = model.docurl || common.getImproveTheDocHref(model, model._gitContribute, model._gitUrlPattern);
  return model;
};

