
function Tree(nodes) {
  this.nodes = nodes;
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
Tree.prototype.firstChild = function (nodeIdx) {
  var n = this.nodes[nodeIdx];
  return n.leaf ? null : this.nodes[nodeIdx + 1];
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
Tree.prototype.children = function (nodeIdx) {
  var n = this.nodes[nodeIdx];
  if (n.leaf)
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

  return prevNode;
}

Tree.prototype.first = function (filter) {
  if (this.nodes.length === 0)
    return null;
  return this.next(-1, filter);
}

