// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

exports.preTransform = function (model) {
  return model;
}

exports.postTransform = function (model) {
  // we need id's for the expanders to link to
  // let's use name + level (that should be unique)
  setItemUid(model);
  return model;
}

function setItemUid(item) {
  if (item.leaf)
    return;
  if (item.name && (!item.uid || item.uid.length === 0))
    item.uid = item.name.replace(/ /g, "_");
  if (item.uid)
    item.link = item.uid.replace(/\./g, "\\.")
  for (var i = 0; i < item.items.length; i++) {
    setItemUid(item.items[i]);
  };
}
