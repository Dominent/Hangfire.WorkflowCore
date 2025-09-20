#!/usr/bin/env node

const { spawn } = require('cross-spawn');
const chalk = require('chalk');
const path = require('path');
const { program } = require('commander');

program
  .option('-c, --configuration <config>', 'Test configuration (Debug|Release)', 'Release')
  .option('-v, --verbosity <level>', 'Verbosity level', 'minimal')
  .option('-w, --watch', 'Run tests in watch mode')
  .option('--filter <pattern>', 'Filter tests by pattern')
  .option('--collect-coverage', 'Collect code coverage')
  .parse();

const options = program.opts();

console.log(chalk.blue('🧪 Running Hangfire.WorkflowCore Tests...'));
console.log(chalk.gray(`Configuration: ${options.configuration}`));
console.log(chalk.gray(`Verbosity: ${options.verbosity}`));

async function runCommand(command, args, description) {
  return new Promise((resolve, reject) => {
    console.log(chalk.yellow(`\n▶️  ${description}`));
    
    const child = spawn(command, args, {
      stdio: 'inherit',
      cwd: path.resolve(__dirname, '..'),
      shell: process.platform === 'win32'
    });

    child.on('close', (code) => {
      if (code === 0) {
        console.log(chalk.green(`✅ ${description} completed successfully`));
        resolve();
      } else {
        console.log(chalk.red(`❌ ${description} failed with exit code ${code}`));
        reject(new Error(`${description} failed`));
      }
    });
  });
}

async function runTests() {
  try {
    const testArgs = [
      'test',
      '--configuration', options.configuration,
      '--verbosity', options.verbosity,
      '--no-restore',
      '--no-build'
    ];

    if (options.filter) {
      testArgs.push('--filter', options.filter);
    }

    if (options.watch) {
      testArgs.push('--watch');
    }

    if (options.collectCoverage) {
      testArgs.push('--collect-coverage');
      testArgs.push('--coverage-output-format', 'cobertura');
      testArgs.push('--coverage-output', 'coverage');
    }

    await runCommand('dotnet', testArgs, 'Running tests');

    if (!options.watch) {
      console.log(chalk.green('\n🎉 All tests passed!'));
      
      // Show test summary
      console.log(chalk.blue('\n📊 Test Summary:'));
      console.log(chalk.gray('  Run `npm run test -- --verbosity normal` for detailed output'));
      
      if (options.collectCoverage) {
        console.log(chalk.blue('\n📈 Coverage report generated in coverage/ directory'));
      }
    }

  } catch (error) {
    console.error(chalk.red('\n💥 Tests failed!'));
    console.error(chalk.gray('Run with --verbosity normal for more details'));
    process.exit(1);
  }
}

runTests();