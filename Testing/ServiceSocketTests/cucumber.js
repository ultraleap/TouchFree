var common = [
	`--format ${
	 process.env.CI || !process.stdout.isTTY ? 'progress' : 'progress-bar'
	 }`,
	 '--format usage:usage.txt',
	 '--format json:./results/test_results.json',
	 '--require-module ts-node/register',
	 '--require ./step_definitions/**/*.ts',
	 '--require ./step_definitions/*.ts',
	//  '--require ./support/**/*.ts',
	//  '--require ./support/*.ts'
	 '--publish-quiet',
	].join(' ');

module.exports = {
	default: common,
};