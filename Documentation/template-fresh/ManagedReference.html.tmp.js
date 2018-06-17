// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

var common = require('./common.js');
var mrefCommon = require('./ManagedReference.common.js');

exports.transform = function (model) {

  model.dataPath = model.baseDir;

  if (mrefCommon && mrefCommon.transform) {
    model = mrefCommon.transform(model);
  }
  if (model.type.toLowerCase() === "enum") {
    model.isClass = false;
    model.isEnum = true;
  }
  model._disableToc = model._disableToc || !model._tocPath || (model._navPath === model._tocPath);

  return model;
}

exports.getOptions = function (model) {
  return {
    "bookmarks": mrefCommon.getBookmarks(model)
  };
}
