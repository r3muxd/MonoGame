// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

var mrefCommon = require('./ManagedReference.common.js');

exports.transform = function (model) {

  if (mrefCommon && mrefCommon.transform)
    model = mrefCommon.transform(model);

  model._disableToc = model._disableToc || !model._tocPath || (model._navPath === model._tocPath);

  return model;
}

exports.getOptions = function (model) {
  return {
    "bookmarks": mrefCommon.getBookmarks(model)
  };
}
