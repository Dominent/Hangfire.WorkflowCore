#!/usr/bin/env node

const { spawn } = require('cross-spawn');
const chalk = require('chalk');
const { program } = require('commander');

program
  .option('--fix', 'Fix formatting and style issues instead of just checking')
  .parse();

const options = program.opts();

if (options.fix) {
  console.log(chalk.blue('🔧 Fixing code formatting and style issues...'));
  
  const lintFixProcess = spawn('dotnet', ['format'], {
    stdio: 'inherit',
    shell: true
  });

  lintFixProcess.on('close', (code) => {
    if (code === 0) {
      console.log(chalk.green('✅ Code formatting and style issues fixed!'));
    } else {
      console.log(chalk.red('❌ Failed to fix some issues.'));
      process.exit(code);
    }
  });

  lintFixProcess.on('error', (err) => {
    console.error(chalk.red('Error running lint fix:'), err);
    process.exit(1);
  });
} else {
  console.log(chalk.blue('🔍 Running code linting...'));

  const lintProcess = spawn('dotnet', ['format', '--verify-no-changes', '--verbosity', 'diagnostic'], {
    stdio: 'inherit',
    shell: true
  });

  lintProcess.on('close', (code) => {
    if (code === 0) {
      console.log(chalk.green('✅ Code linting passed!'));
    } else {
      console.log(chalk.red('❌ Code linting failed. Run "npm run lint:fix" to auto-fix issues.'));
      process.exit(code);
    }
  });

  lintProcess.on('error', (err) => {
    console.error(chalk.red('Error running linter:'), err);
    process.exit(1);
  });
}