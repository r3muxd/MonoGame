
var common = require("./common.js");

exports.transform = function (model) {
  model.baseDir = model.baseDir || '';
  model.dataPath = common.getFileNameWithoutExtension(model._key);
  model.hasToc = !model.disableToc && model._navRel != model._tocRel;
  model.docurl = model.docurl || common.getImproveTheDocHref(model, model._gitContribute, model._gitUrlPattern);
  return model;
};

