
var common = require("./common.js");

exports.transform = function (model) {
  model.dataPath = getDataPath(model._key);
  model.hasToc = !model.disableToc && model._navRel != model._tocRel;
  model.docurl = model.docurl || common.getImproveTheDocHref(model, model._gitContribute, model._gitUrlPattern);
  return model;

  function getDataPath(path) {
    var filename = common.getFileNameWithoutAnyExtensions(path);
    return filename + ".json";
  }
};

