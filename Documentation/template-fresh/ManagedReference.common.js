// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.
var common = require('./common.js');

exports.transform = function (model)  {
  if (!model) return null;

  langs = model.langs;
  handleItem(model, model._gitContribute, model._gitUrlPattern);
  if (model.children) {
    model.children.forEach(function (item) {
      handleItem(item, model._gitContribute, model._gitUrlPattern);
    });
  }

  if (!model.type) {
    console.err('Managed Reference without a type! uid: ' + model.uid);
    return model;
  }

  var lcType = model.type.toLowerCase();
  var cat = getCategory(lcType);
  var typePropName = getTypePropertyName(lcType);

  model[cat] = true;
  model[typePropName] = true;
  model.title = getTitle(model);

  // the structure of these isn't consistent, possibly due to the member split plugin not being up to date
  // for class members that don't have overload pages we prepend the parent name
  // for class members on overload pages the title is fixed below
  if (cat === 'classMember' && !model.children && model.parent && model.parent.name)
    model.title = model.parent.name[0].value + '.' + model.title;

  model.hideChildren = true;
  model.subHeaderLevel = 2;

  if (model.children && model.children.length > 0) {
    if (cat === 'ns' || (cat === 'namespaceMember' && model._splitReference)) {
      model.isCollection = true;
      groupChildren(model, cat);
    } else if (cat === 'classMember') {

      model.expandChildren = true;
      model.children.forEach(function (item) {
        var parentName = item.parent.substring(item.parent.lastIndexOf('.'));
        if (!item.title)
          item.title = parentName + '.' + getTitle(item);
        item.subHeaderLevel = 3;
        item.expandChildren = false;
        item.isCollection = false;
      });
 
      if (model.children.length > 1) {
        model.childTypes = [ { 'id': 'overloads', 'inOverload': true, 'children': model.children} ];
        model.isCollection = true;
      } else {
        // overload pages are generated even though there's only 1 member
        // we work around this by expanding the child member as if it were a root item
        var child = model.children[0];
        model.title = child.title;
        child.skipOverloadTitle = true;
        child.subHeaderLevel = 2;
      }
    }
  }

  return model;
}

exports.getBookmarks = function (model)  {
  if (!model.type)
    console.err('Managed Reference without a type! uid: ' + model.uid);

  var bookmarks = {};

  // Reference's first level bookmark should have no anchor
  bookmarks[model.uid] = "";

  var cat = getCategory(model.type.toLowerCase());
  if (cat === 'ns' || model._splitReferences && cat === 'namespaceMember')
    return bookmarks;

  if (model.children) {
    model.children.forEach(function (item) {
      bookmarks[item.uid] = common.getHtmlId(item.uid);
      if (item.overload && item.overload.uid)
        bookmarks[item.overload.uid] = common.getHtmlId(item.overload.uid);
    });
  }
  return bookmarks;
}

exports.getTitle = getTitle;

function getTitle(item) {
  if (!item.type)
    console.err('Item without type: ' + item.uid);

  var title = item.name[0].value;
  var lcType = item.type.toLowerCase();
  var typeStr = '';
  switch (lcType) {
    case 'class':
      typeStr = 'Class';
      break;
    case 'struct':
      typeStr = 'Struct';
      break;
    case 'interface':
      typeStr = 'Interface';
      break;
    case 'enum':
      typeStr = 'Enum';
      break;
    case 'delegate':
      typeStr = 'Delegate';
      break;
    case 'constructor':
      typeStr = 'Constructor';
      break;
    case 'field':
      typeStr = 'Field';
      break;
    case 'property':
      typeStr = 'Property';
      break;
    case 'event':
      typeStr = 'Event';
      break;
    case 'operator':
      typeStr = 'Operator';
      break;
    case 'method':
    case 'eii':
      typeStr = 'Method';
      break;
    default:
      break;
  }

  return title + ' ' + typeStr;
}

function groupChildren(model, category) {

  var childTypeDefs = getDefinitions(category);
  var grouped = {};

  model.children.forEach(function (c) {
    var type = c.type.toLowerCase();
    if (!grouped.hasOwnProperty(type))
      grouped[type] = [];

    // special handle for field
    if (type === "field" && c.syntax) {
      c.syntax.fieldValue = c.syntax.return;
      c.syntax.return = undefined;
    }
    // special handle for property
    if (type === "property" && c.syntax) {
      c.syntax.propertyValue = c.syntax.return;
      c.syntax.return = undefined;
    }
    // special handle for event
    if (type === "event" && c.syntax) {
      c.syntax.eventType = c.syntax.return;
      c.syntax.return = undefined;
    }
    grouped[type].push(c);
  });

  var childTypeItems = [];
  for (var childType in childTypeDefs) {
    if (!childTypeDefs.hasOwnProperty(childType) || !grouped.hasOwnProperty(childType))
      continue;

    var typeDef = childTypeDefs[childType];

    var childTypeItem = {};
    childTypeItem[typeDef.typePropertyName] = true;
    childTypeItem.id = typeDef.id;
    childTypeItem.children = grouped[childType];

    childTypeItems.push(childTypeItem);
  }

  model.childTypes = childTypeItems;
}

var namespaceMembers = {
  "class":          { typePropertyName: "inClass",        id: "classes" },
  "struct":         { typePropertyName: "inStruct",       id: "structs" },
  "interface":      { typePropertyName: "inInterface",    id: "interfaces" },
  "enum":           { typePropertyName: "inEnum",         id: "enums" },
  "delegate":       { typePropertyName: "inDelegate",     id: "delegates" }
};

var classMembers = {
  "constructor":    { typePropertyName: "inConstructor",  id: "constructors" },
  "field":          { typePropertyName: "inField",        id: "fields" },
  "property":       { typePropertyName: "inProperty",     id: "properties" },
  "method":         { typePropertyName: "inMethod",       id: "methods" },
  "event":          { typePropertyName: "inEvent",        id: "events" },
  "operator":       { typePropertyName: "inOperator",     id: "operators" },
  "eii":            { typePropertyName: "inEii",          id: "eii" }
};

function getTypePropertyName(type) {
  if (type === "namespace")
    return 'inNamespace';

  var def = classMembers[type] || namespaceMembers[type];

  if (!def)
    console.err('Unknown type: ' + type);

  return def.typePropertyName;
}

function getCategory(type) {
  if (type === 'namespace')
    return 'ns';
  if (namespaceMembers.hasOwnProperty(type))
    return 'namespaceMember';
  if (classMembers.hasOwnProperty(type))
    return 'classMember';
  console.err('Type without category: ' + type);
}
 
function getDefinitions(category) {
  if (category === 'ns')
    return namespaceMembers;
  if (category === 'namespaceMember')
    return classMembers;

  console.err("category '" + category + "' is not valid.");
}

function handleItem(vm, gitContribute, gitUrlPattern) {
  // get contribution information
  vm.docurl = common.getImproveTheDocHref(vm, gitContribute, gitUrlPattern);
  vm.sourceurl = common.getViewSourceHref(vm, null, gitUrlPattern);

  // set to null incase mustache looks up
  vm.summary = vm.summary || null;
  vm.remarks = vm.remarks || null;
  vm.conceptual = vm.conceptual || null;
  vm.syntax = vm.syntax || null;
  vm.implements = vm.implements || null;
  vm.example = vm.example || null;
  common.processSeeAlso(vm);

  // id is used as default template's bookmark
  vm.id = common.getHtmlId(vm.uid);
  if (vm.overload && vm.overload.uid) {
    vm.overload.id = common.getHtmlId(vm.overload.uid);
  }

  if (vm.supported_platforms) {
      vm.supported_platforms = transformDictionaryToArray(vm.supported_platforms);
  }

  if (vm.requirements) {
      var type = vm.type.toLowerCase();
      if (type == "method") {
          vm.requirements_method = transformDictionaryToArray(vm.requirements);
      } else {
          vm.requirements = transformDictionaryToArray(vm.requirements);
      }
  }

  if (vm && langs) {
      if (shouldHideTitleType(vm)) {
          vm.hideTitleType = true;
      } else {
          vm.hideTitleType = false;
      }

      if (shouldHideSubtitle(vm)) {
          vm.hideSubtitle = true;
      } else {
          vm.hideSubtitle = false;
      }
  }

  function shouldHideTitleType(vm) {
      var type = vm.type.toLowerCase();
      return ((type === 'namespace' && langs.length == 1 && (langs[0] === 'objectivec' || langs[0] === 'java' || langs[0] === 'c'))
      || ((type === 'class' || type === 'enum') && langs.length == 1 && langs[0] === 'c'));
  }

  function shouldHideSubtitle(vm) {
      var type = vm.type.toLowerCase();
      return (type === 'class' || type === 'namespace') && langs.length == 1 && langs[0] === 'c';
  }

  function transformDictionaryToArray(dic) {
    var array = [];
    for(var key in dic) {
        if (dic.hasOwnProperty(key)) {
            array.push({"name": key, "value": dic[key]})
        }
    }

    return array;
  }
}
