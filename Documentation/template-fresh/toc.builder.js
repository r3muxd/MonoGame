// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

var common = require('./common.js');

exports.transform = function (model) {
  transformItem(model, 1);

  function transformItem(item, level) {
    if (item.topicHref) {
      var dir = common.getDirectory(model._path);
      var subPath = item.topicHref.slice(0, item.topicHref.indexOf("."));
      // todo base dir
      item.dst = "/" + dir + subPath + ".html";
    }

    if (item.tocHref) {
      item.navName = common.getNavId(item.tocHref);
      item.filename = common.getFileNameWithoutAnyExtensions(item.tocHref);
    } else if (item.href) {
      item.navName = common.getNavId(item.href);
      item.filename = common.getFileNameWithoutAnyExtensions(item.href);
    }

    item.level = level;

    if (!item.items)
      item.items = [];

    item.leaf = item.items.length === 0;

    item.items.forEach(function (child) {
      transformItem(child, level + 1);
    });
  }
};

