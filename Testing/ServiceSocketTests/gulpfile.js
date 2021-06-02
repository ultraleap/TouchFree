const { spawn } = require('child_process');
const { chmodSync, readFileSync } = require('fs');

const fs = require('fs');
const Tail = require('tail').Tail;

var del = require('del');
var gulp = require('gulp');

// A child process used for the start/stopping of the server
var serverProcess;

function cucumberXmlReport(opts) {
    var gutil = require('gulp-util'),
        through = require('through2'),
        cucumberJunit = require('cucumber-junit');

    return through.obj(function (file, enc, cb) {
        // If tests are executed against multiple browsers/devices
        var suffix = file.path.match(/\/cucumber-?(.*)\.json/);
        if (suffix) {
            opts.prefix = suffix[1] + ';';
        }

        var xml = cucumberJunit(file.contents, opts);
        file.contents = new Buffer.from(xml);
        file.path = gutil.replaceExtension(file.path, '.xml');
        cb(null, file);
    });
};

gulp.task('buildServerGrab', function () {
    return gulp.src([
        '../../Build/**',
        '../../Build/*',
    ])
        .pipe(gulp.dest('./PUT_TEST_BUILD_IN_HERE'));
})

gulp.task('startServer', function (callback) {
    var serverBinDir = `./PUT_TEST_BUILD_IN_HERE`;
    var logFileName = `log.txt`;
    var logFileLoc = `${serverBinDir}/${logFileName}`;

    var startCommand = "./ScreenControlService";

    if (process.platform === "win32") {
        startCommand = `.\\TouchFree_Service.exe`;
        serverBinDir = serverBinDir.replace(/\//g, '\\');
        logFileLoc = logFileLoc.replace(/\//g, '\\');
    } else {
        chmodSync(serverBinDir + startCommand, 0o765, (err) => {
            callback("The permissions for the haptic server could not be set!");
        });
    }

    if (!fs.existsSync(logFileLoc)) {
        fs.writeFileSync(logFileLoc, ' ');
    }

    function checkLogForReady() {
        return new Promise(function(resolve, reject) {
            const fileContent = fs.readFileSync(logFileLoc);
            console.log("Checking file content");

            if (fileContent.toString().includes("Service Setup Complete")) {
                console.log("Server ready!");
                callback();
                return resolve();
            }

            console.log("Server not ready");

            return new Promise(() => {
                setTimeout(() => {
                    return checkLogForReady();
                }, 1000);
            });
        });
    }

    console.log(`Attempting to run command ${startCommand} in target dir ${serverBinDir}`);

    serverProcess = spawn(startCommand,
        [
            '-batchmode',
            '-logfile',
            `${logFileName}`
        ],
        { 'cwd': serverBinDir });

        serverProcess.on('close', () => {
            callback('Server process closed')
        });

    checkLogForReady();
});

gulp.task('cucumber', function (callback) {
    var nodeBinDir = "./node_modules/.bin/";
    var startCommand;

    if (process.platform === "win32") {
        var startCommand = `${nodeBinDir}\cucumber-js`;
    } else {
        var startCommand = `${nodeBinDir}/cucumber-js.cmd`;

        chmodSync(nodeBinDir + startCommand, 0o765, (err) => {
            callback("The permissions for the haptic server could not be set!");
        });
    }

    console.log(`Attempting to run command ${startCommand} in target dir ${nodeBinDir}`);

    let cucumberProcess = spawn(
        'node',
        [
            './node_modules/@cucumber/cucumber/bin/cucumber-js'
        ],
        {
            stdio: "inherit"
        });

    return cucumberProcess;
});

gulp.task('cucumber:report', function () {
    return gulp.src('./results/test_results.json')
        .pipe(cucumberXmlReport({ strict: true }))
        .pipe(gulp.dest('./results'));
});

gulp.task('killServer', function (callback) {
    serverProcess.on('close', (code, signal) => {
        console.log("Successfully closed server");
        callback();
    });

    serverProcess.kill();
});

gulp.task('cleanLocalArtefacts', function () {
    return del([
        './PUT_TEST_BUILD_IN_HERE/*'
    ]);
});

gulp.task('checkResults', function (callback) {
    let rawdata = readFileSync(`./results/test_results.json`);
    let parsedData = JSON.parse(rawdata);
    let foundIssue = false;

    parsedData.forEach((feature) => {
        feature.elements.forEach((element) => {
            element.steps.forEach((step) => {
                if (step.result.status !== "passed") {
                    foundIssue = true;
                }
            });
        })
    });

    if (foundIssue) {
        callback("Found a failing test");
    } else {
        callback();
    }
});

function localRun() {
    return gulp.series(
        'startServer',
        'cucumber',
        'cucumber:report',
        'killServer',
        'checkResults'
    );
}

gulp.task('default', localRun());
gulp.task('local_run', localRun());

gulp.task('build_machine_run',
    gulp.series(
        'cleanLocalArtefacts',
        'buildServerGrab',
        'startServer',
        'cucumber',
        'cucumber:report',
        'killServer',
        'checkResults'
    ));
