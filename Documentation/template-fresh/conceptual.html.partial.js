
var common = require("./common.js");

exports.transform = function (model) {
  model.docurl = model.docurl || common.getImproveTheDocHref(model, model._gitContribute, model._gitUrlPattern);
  fixImageLinks();
  return model;

  function fixImageLinks() {
    var html = model.conceptual;
    var i = html.length - 1;
    while ((i = html.lastIndexOf("<img src=", i)) != -1) {
      var start = html.indexOf('"', i) + 1;
      var end = html.indexOf('"', start + 1);
      html = fixImageLink(html, start, end);
      i--; // don't rematch
    }

    model.conceptual = html;
  }

  function fixImageLink(html, start, end) {
    // docfx fails to resolve image links because of the dynamic content
    // image src will look like: "~/manual/getting_started/images/1_new_solution_vs.png"
    // while we just need the relative path: "images/1_new_solution_vs.png"
    // so let's fix that :)
    var og = html.substring(start, end);
    var abs = og.substring(2); // '~/path' to 'path'
    var rel = makeRelative(model._path, abs);

    return html.substring(0, start) + rel + html.substring(end);
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

