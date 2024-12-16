(function(){

// need to fix line 1946
// from   var index = keys.indexOf(id, index);
// to:    var index = keys.indexOf(id, 0);
// propably a browser bug since index wasn't initialized yet...


var WebSqlPouchCore = require('pouchdb-adapter-websql-core')

'use strict'


var assign
if (typeof Object.assign === 'function') {
	assign = Object.assign
} else {
	// lite Object.assign polyfill based on
	// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/assign
	assign = function (target) {
		var to = Object(target)

		for (var index = 1; index < arguments.length; index++) {
			var nextSource = arguments[index]

			if (nextSource != null) { // Skip over if undefined or null
				for (var nextKey in nextSource) {
					// Avoid bugs when hasOwnProperty is shadowed
					if (Object.prototype.hasOwnProperty.call(nextSource, nextKey)) {
						to[nextKey] = nextSource[nextKey]
					}
				}
			}
		}
		return to
	}
}













/* global cordova, sqlitePlugin, openDatabase */
function createOpenDBFunction (opts) {
	return function (name, version, description, size) {
		var newOpts = assign({}, opts, {
			name: name,
			version: version,
			description: description,
			size: size
		})
		return xbridge.websql.openDatabase(newOpts)
	}
}

function XBridgeSQLitePouch (opts, callback) {
	var websql = createOpenDBFunction(opts)
	var _opts = assign({
		websql: websql
	}, opts)
	/*
	if (openDatabase === 'undefined') {
		console.error(
			'PouchDB error: you must install a SQLite plugin ' +
			'websql\ API function openDatabase is not available')
	}*/

	if ( 'default' in WebSqlPouchCore && typeof WebSqlPouchCore.default.call === 'function') {
		WebSqlPouchCore.default.call(this, _opts, callback)
	} else {
		WebSqlPouchCore.call(this, _opts, callback)
	}
}

XBridgeSQLitePouch.valid = function () {
	// if you're using Cordova, we assume you know what you're doing because you control the environment
	return true
}

// no need for a prefix in cordova (i.e. no need for `_pouch_` prefix
XBridgeSQLitePouch.use_prefix = false

function xbridgeSqlitePlugin (PouchDB) {
	PouchDB.adapter('xbridge.pouch', XBridgeSQLitePouch, true)
}

if (typeof window !== 'undefined' && window.PouchDB) {
	window.PouchDB.plugin(xbridgeSqlitePlugin)
}

if(!xbridge)
	throw new Error("could not load pouch adapter xbridge.pouch. xbridge not available");
if(!xbridge.websql)
	throw new Error("could not load pouch adapter xbridge.pouch. xbridge-websql not available");
xbridge.pouch = xbridgeSqlitePlugin


})();