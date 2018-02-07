var common = require('./common.js')

exports.getOptions = function (model) {
  return {
    isShared: true
  };
}

exports.transform = function (model) {

  var nodes = buildTree(model);
  model.toc = JSON.stringify(nodes);
  return model;
}

// builds a flat tree of the toc items; nodes are stored in one array for easy lookups
// node has properties { parent, leaf, index, prevSibling, nextSibling } for navigation
// and properties { level, name, path } as data
function buildTree(model) {
  var items = model.items;

  addIndices(items);
  var nodes = [];
  buildTreeRec(nodes, null, items, 1);
  return nodes;

  function buildTreeRec(nodes, parent, items, level) {
    for (var i = 0; i < items.length; i++) {
      var item = items[i];

      var leaf = !item.items || item.items.length === 0;
      var index = item.index;
      var prevSibling = i - 1 >= 0 ? items[i-1].index : null;
      var nextSibling = i + 1 < items.length ? items[i+1].index : null;

      var path = null;
      if (item.topicHref) {
        var dir = common.getDirectory(model._path);
        var subPath = item.topicHref.slice(0, item.topicHref.indexOf("."));
        path = '/' + dir + subPath;
      }

      var node = {
        parent: parent,
        leaf: leaf,
        index: index,
        prevSibling: prevSibling,
        nextSibling: nextSibling,

        level: level,
        name: item.name,
        path: path
      };

      nodes.push(node);
      if (!leaf)
        buildTreeRec(nodes, index, item.items, level + 1);
    }
  }
}

// assign depth-first indices to all tree items so we can easily build the flat tree
function addIndices(items) {
  addIndicesRec(items, {index: 0});

  function addIndicesRec(items, indexBox) {
    for (var i = 0; i < items.length; i++) {
      var item = items[i];
      item.index = indexBox.index;
      indexBox.index += 1;
      if (item.items)
        addIndicesRec(item.items, indexBox);
    }
  }
}

