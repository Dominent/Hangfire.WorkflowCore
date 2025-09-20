#!/usr/bin/env node

const { spawn } = require('cross-spawn');
const chalk = require('chalk');
const { program } = require('commander');
const fs = require('fs-extra');
const path = require('path');

program
  .option('--report', 'Generate HTML coverage report')
  .option('--open', 'Open coverage report in browser (implies --report)')
  .parse();

const options = program.opts();

// Ensure coverage directory exists
const coverageDir = path.join(process.cwd(), 'coverage');
fs.ensureDirSync(coverageDir);

console.log(chalk.blue('📊 Running code coverage analysis...'));

// Run tests with code coverage
const coverageProcess = spawn('dotnet', [
  'test',
  '--configuration', 'Release',
  '--collect:"XPlat Code Coverage"',
  '--results-directory', './coverage'
], {
  stdio: 'inherit',
  shell: true
});

coverageProcess.on('close', (code) => {
  if (code !== 0) {
    console.log(chalk.red('❌ Coverage analysis failed.'));
    process.exit(code);
  }

  console.log(chalk.green('✅ Coverage analysis completed!'));

  // Always generate a merged coverage report for CI
  console.log(chalk.blue('📈 Generating merged coverage report...'));
  
  const mergeProcess = spawn('reportgenerator', [
    '-reports:./coverage/**/coverage.cobertura.xml',
    '-targetdir:./coverage/merged',
    '-reporttypes:Cobertura',
    '-assemblyfilters:-*.Tests*'
  ], {
    stdio: 'inherit',
    shell: true
  });

  mergeProcess.on('close', (mergeCode) => {
    if (mergeCode === 0) {
      console.log(chalk.green('✅ Merged coverage report generated!'));
    } else {
      console.log(chalk.yellow('⚠️  Could not generate merged coverage report (reportgenerator not available)'));
    }

    if (options.report || options.open) {
      console.log(chalk.blue('📈 Generating HTML coverage report...'));
    
    const reportProcess = spawn('reportgenerator', [
      '-reports:./coverage/**/coverage.cobertura.xml',
      '-targetdir:./coverage/report',
      '-reporttypes:Html'
    ], {
      stdio: 'inherit',
      shell: true
    });

    reportProcess.on('close', (reportCode) => {
      if (reportCode === 0) {
        console.log(chalk.green('✅ HTML coverage report generated at ./coverage/report/index.html'));
        
        if (options.open) {
          console.log(chalk.blue('🌐 Opening coverage report in browser...'));
          const openProcess = spawn('open', ['./coverage/report/index.html'], {
            stdio: 'inherit',
            shell: true
          });

          openProcess.on('error', (err) => {
            console.log(chalk.yellow('⚠️  Could not auto-open browser. Please open ./coverage/report/index.html manually.'));
          });
        }
      } else {
        console.log(chalk.red('❌ Failed to generate coverage report.'));
        process.exit(reportCode);
      }
    });

    reportProcess.on('error', (err) => {
      console.error(chalk.red('Error generating coverage report:'), err);
      console.log(chalk.yellow('Make sure reportgenerator is installed: dotnet tool install -g dotnet-reportgenerator-globaltool'));
      process.exit(1);
    });
    }
  });
});

coverageProcess.on('error', (err) => {
  console.error(chalk.red('Error running coverage analysis:'), err);
  process.exit(1);
});