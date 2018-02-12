
if (typeof exports !== 'undefined') {
  exports.TreeBuilder = TreeBuilder;
  exports.Tree = Tree;
}

function TreeBuilder() {
  this.lastAtDepth = [];
  this.nodes = [];
}

TreeBuilder.prototype.push = function (node, depth) {
  var i = this.nodes.length;

  // console.log('Pushing ' + i + '; depth: ' + depth + '; [' + this.lastAtDepth + ']');
  // console.log('Node: ' + JSON.stringify(node));

  node.index = i;
  node.depth = depth;
  node.parent = depth - 1 >= 0 ? this.lastAtDepth[depth - 1] : null;
  node.prevSibling = depth < this.lastAtDepth.length ? this.lastAtDepth[depth] : null;
  node.nextSibling = null;
  if (node.prevSibling !== null)
    this.nodes[node.prevSibling].nextSibling = i;

  // console.log('Result: ' + JSON.stringify(node));

  this.nodes.push(node);
  this.lastAtDepth[depth] = i;
  this.lastAtDepth = this.lastAtDepth.slice(0, depth + 1);
}

TreeBuilder.prototype.finish = function () {
  return new Tree(this.nodes);
}

function Tree(nodes) {
  this.nodes = nodes;
}

Tree.prototype.size = function () {
  return this.nodes.length;
}

Tree.prototype.parent = function (nodeIdx) {
  var n = this.nodes[nodeIdx];
  return n.parent ? this.nodes[n.parent] : null;
}
Tree.prototype.prevSibling = function (nodeIdx) {
  var n = this.nodes[nodeIdx];
  return n.prevSibling ? this.nodes[n.prevSibling] : null;
}
Tree.prototype.nextSibling = function (nodeIdx) {
  var n = this.nodes[nodeIdx];
  return n.nextSibling ? this.nodes[n.prevSibling] : null;
}
Tree.prototype.hasChildren = function (nodeIdx) {
  var n = this.nodes[nodeIdx];
  if (nodeIdx + 1 >= this.nodes.length)
    return false;
  return this.nodes[nodeIdx + 1].depth > n.depth;
}
Tree.prototype.rootNodes = function () {
  if (this.nodes.length === 0)
    return [];

  var rns = [];
  var item = 0;
  while (item != null) {
    rns.push(this.nodes[item]);
    item = this.nodes[item].nextSibling;
  }
  return rns;
}
Tree.prototype.rootParent = function (nodeIdx) {
  var node = this.nodes[nodeIdx];
  while (node.parent !== null)
    node = this.nodes[node.parent];
  return node;
}
Tree.prototype.children = function (nodeIdx) {
  var n = this.nodes[nodeIdx];
  if (!this.hasChildren(nodeIdx))
    return [];

  var cs = [];
  var child = nodeIdx + 1;
  while (child != null) {
    cs.push(this.nodes[child]);
    child = this.nodes[child].nextSibling;
  }
  return cs;
}

Tree.prototype.prev = function (currentIdx, filter) {
  if (currentIdx === 0)
    return null;

  var prevNode = this.nodes[currentIdx - 1];

  if (filter && !filter(prevNode))
    return this.prev(currentIdx - 1, filter);

  return prevNode;
}

Tree.prototype.next = function (currentIdx, filter) {
  if (currentIdx === this.nodes.length - 1)
    return null;
  
  var nextNode = this.nodes[currentIdx + 1];

  if (filter && !filter(nextNode))
    return this.next(currentIdx + 1, filter);

  return nextNode;
}

Tree.prototype.first = function (filter) {
  if (this.nodes.length === 0)
    return null;
  return this.next(-1, filter);
}
Tree.prototype.last = function (filter) {
  if (this.nodes.length === 0)
    return null;
  return this.prev(this.nodes.length, filter);
}

Tree.prototype.doAll = function (f) {
  for (var i = 0; i < this.nodes.length; i++)
    f(this.nodes[i]);
}
Tree.prototype.doSelf = function (nodeIdx, f) {
  f(this.nodes[nodeIdx]);
}
Tree.prototype.doBelow = function (nodeIdx, f) {
  // apply 'f' to all nodes below the node at 'nodeIdx' and return the index of the last applied node
  var rootDepth = this.nodes[nodeIdx].depth;
  for (var i = nodeIdx + 1; i < this.nodes.length && this.nodes[i].depth > rootDepth; i++)
    f(this.nodes[i]);
  return i - 1;
}
Tree.prototype.doAncestors = function (nodeIdx, f) {
  for (var i = this.nodes[nodeIdx].parent; i != null; i = this.nodes[i].parent)
    f(this.nodes[i]);
}

