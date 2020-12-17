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
    var serverBinDir = "./PUT_TEST_BUILD_IN_HERE/";
    var startCommand;


    if (!fs.existsSync("./PUT_TEST_BUILD_IN_HERE/log.txt"))
    {
        fs.writeFileSync("./PUT_TEST_BUILD_IN_HERE/log.txt", ' ');
    }

    tail = new Tail("./PUT_TEST_BUILD_IN_HERE/log.txt");

    tail.on("line", function(data) {
        console.log(data);

        if (data.includes("Service Setup Complete")) {
            callback();
        }
    });

    tail.on("error", function(error) {
        console.log('ERROR: ', error);
    });

    if (process.platform === "win32") {
        serverBinDir = serverBinDir.replace(/\//g, '\\');
        startCommand = `.\\ScreenControlService.exe`;
    } else {
        startCommand = "./ScreenControlService";

        chmodSync(serverBinDir + startCommand, 0o765, (err) => {
            callback("The permissions for the haptic server could not be set!");
        });
    }

    console.log(`Attempting to run command ${startCommand} in target dir ${serverBinDir}`);

    serverProcess = spawn(startCommand,
        [
            '-batchmode',
            '-logfile',
            'log.txt'
        ],
        { 'cwd': serverBinDir });

    serverProcess.on('close', () => {
        callback('Server process closed')
    });
});

gulp.task('cucumber', function (callback) {
    let tags = 'not @Manual and not @Managed and not @SimulatedOutput';

    if (process.platform == "darwin") {
        tags += ' and not @SimulatedOutput';
    }

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
