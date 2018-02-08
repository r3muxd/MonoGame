var common = require('./common.js');
var tree = require('./js/tree.js');

exports.getOptions = function (model) {
  return {
    isShared: true
  };
}

exports.transform = function (model) {

  var tocTree = buildTree(model);
  model.toc = JSON.stringify(tocTree.nodes);
  return model;
}

// builds a flat tree of the toc items; nodes are stored in one array for easy lookups
// node has properties { parent, leaf, index, prevSibling, nextSibling } for navigation
// and properties { level, name, path } as data
function buildTree(model) {
  var items = model.items;

  var nodes = [];
  var builder = new tree.TreeBuilder();
  buildTreeRec(items, 0);
  return builder.finish();

  function buildTreeRec(items, depth) {
    for (var i = 0; i < items.length; i++) {
      var current = items[i];

      var path = null;
      if (current.topicHref) {
        var dir = common.getDirectory(model._path);
        var subPath = common.stripExtension(current.topicHref);
        path = '/' + dir + subPath;
      }

      var node = { name: current.name, path: path };
      builder.push(node, depth);
      if (current.items)
        buildTreeRec(current.items, depth + 1);
    }
  }
}


