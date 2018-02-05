var tocbuilder = require('./toc.builder.js')

exports.transform = function (model) {

  tocbuilder.transform(model);
  return model;
}
