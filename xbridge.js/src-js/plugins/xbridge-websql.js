(function(){

var customOpenDatabase = require('websql/custom');

var openDB = customOpenDatabase(SQLiteDatabase);


function map(arr, fun) {
	var len = arr.length;
	var res = Array(len);
	for (var i = 0; i < len; i++) {
		res[i] = fun(arr[i], i);
	}
	return res;
}

function zipObject(props, values) {
	var res = {};
	var len = Math.min(props.length, values.length);
	for (var i = 0; i < len; i++) {
		res[props[i]] = values[i];
	}
	return res;
}




function openDatabase(name, version, description, size, callback) {
	if (name && typeof name === 'object') {
		callback = version;
		size = name.size;
		description = name.description;
		version = name.version;
		name = name.name;
	}
	if (!size) {
		size = 1;
	}
	if (!description) {
		description = name;
	}
	if (!version) {
		version = '1.0';
	}
	if (typeof name === 'undefined') {
		throw new Error('please be sure to call: openDatabase("myname.db")');
	}
	return openDB(name, version, description, size, callback);
}










function SQLiteResult(error, insertId, rowsAffected, rows) {
	this.error = error;
	this.insertId = insertId;
	this.rowsAffected = rowsAffected;
	this.rows = rows;
}










function massageError(err) {
	return typeof err === 'string' ? new Error(err) : err;
}

function SQLiteDatabase(name) {
	this._name = name;
}

function transformResult(res) {
	if (res.error) {
		return new SQLiteResult(massageError(res.error));
	}
	var columns = res.columns;
	if(res.rows)
	{
		var zippedRows = [];
		var rows = res.rows;
		for (var i = 0, len = rows.length; i < len; i++) {
			zippedRows.push(zipObject(columns, rows[i]));
		}
		res.rows = zippedRows;
	}
	var insertId = res.insertId;
	if(insertId == null)
		insertId = void 0;

	return new SQLiteResult(null, insertId, res.rowsAffected, res.rows);
}

function arrayifyQuery(query) {
	return [query.sql, (query.args || [])];
}

SQLiteDatabase.prototype.exec = function exec(queries, readOnly, callback) {
	if(!readOnly)
		readOnly = false;
	var params = [];
	var sqls = [];
	for (var i = queries.length - 1; i >= 0; i--) {
		params[i] = queries[i].args || [];
		sqls[i] = queries[i].sql;
	}
	xbridge.exec('sqlite.execMany', this._name, sqls, params, readOnly).then(function(rawResults){
		if (typeof rawResults === 'string') {
			rawResults = JSON.parse(rawResults);
		}
		var results = map(rawResults, transformResult);
		callback(null, results);
	}).catch(function(err){
		console.error(err);
		callback(massageError(err));
	});
};

if(!xbridge)
	throw new Error("could not load xbridge plugin xbridge-websql. xbridge not available");

xbridge.websql = {
	openDatabase: openDatabase
}


})();
