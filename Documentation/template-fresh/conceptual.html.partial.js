
var common = require("./common.js");

exports.transform = function (model) {
  model.docurl = model.docurl || common.getImproveTheDocHref(model, model._gitContribute, model._gitUrlPattern);
  fixLinks('<a href=', fixAnchorLink);
  fixLinks('<img src=', fixImageLink);
  return model;

  function fixLinks(pattern, process) {
    var html = model.conceptual;
    var i = html.length - 1;
    while ((i = html.lastIndexOf(pattern, i)) != -1) {
      var start = html.indexOf('"', i) + 1;
      var end = html.indexOf('"', start + 1);
      html = fixLink(html, start, end, process);
      i--; // don't rematch
    }

    model.conceptual = html;
  }

  function fixLink(html, start, end, process) {
    // src will look like: "~/manual/getting_started/images/1_new_solution_vs.png"
    // while we just need the relative path: "images/1_new_solution_vs.png"
    // so let's fix that :)
    if (html.substring(start, start + 2) !== '~/')
      return html;

    var og = html.substring(start, end);
    return html.substring(0, start) + process(og) + html.substring(end);
  }

  function fixAnchorLink(link) {
    // ~/articles/getting_started.md to /articles/getting_started.html
    return common.stripExtension(link.slice(1)) + '.html';
  }

  function fixImageLink(link) {
    var abs = link.substring(2); // '~/path' to 'path'
    return makeRelative(model._path, abs);
  }

  function makeRelative(from, to) {
    // first finds common directories, then appends rest of to

    var rel = '';

    var matching = 0;
    var matchingIndex = 0;
    var i = 0;
    while ((i = from.indexOf('/', i)) != -1) {
      i = i + 1; // index right after the separator
      var dir = from.substring(0, i);
      if (to.indexOf(dir) !== 0) // startsWith is not implemented in the jint version used by docfx
        break;
      matching++;
      matchingIndex = i;
    }
    var fromSeparators = (from.match(/\//g) || []).length;
    for (i = 0; i < fromSeparators - matching; i++)
      rel += '../';

    var left = to.substring(matchingIndex);
    return rel + left;
  }
};

