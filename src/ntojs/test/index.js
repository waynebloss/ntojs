var util  = require('util'),
    spawn = require('child_process').spawn;

function runsql(sql, callback) {
    var ntojs = spawn('../bin/Release/ntojs.exe'), //, ['"Data Source=localhost;Initial Catalog=Scratch;Integrated Security=True"']),
        result = '',
        resultSize = 0;

    ntojs.stdin.setEncoding("utf8");

    ntojs.stdout.on('data', function (data) {
        // Append to our buffer
//        resultSize += data.length;
//        data.copy(result, resultSize);
        result = result + data;
    });

    ntojs.stderr.on('data', function (data) {
        // Handle error output
        console.log('error: ' + data);
    });

    ntojs.on('exit', function (code) {
        // Done, trigger your callback (perhaps check `code` here)
        callback(result, resultSize);
    });

//    var buf = new Buffer(4);
//    buf.writeInt32LE(sql.length, 0);

//    ntojs.stdin.write(buf);
    // Write the sql
    ntojs.stdin.write(sql);
    ntojs.stdin.end();
}
//exports.runsql = runsql;

function test() {
    runsql('select count(1) as c from IntSeq;', function (result, resultSize) {
//        var StringDecoder = require('string_decoder').StringDecoder;
//        var decoder = new StringDecoder('utf8');
//        console.log(decoder.write(result, 0, resultSize));
        console.log(result);
        process.exit();
    });
    return 'OK';
}
//exports.test = test;

test();