#!/usr/bin/env node

const { spawn } = require('cross-spawn');
const chalk = require('chalk');
const path = require('path');
const { program } = require('commander');

program
  .option('-c, --configuration <config>', 'Build configuration (Debug|Release)', 'Release')
  .option('-v, --verbosity <level>', 'Verbosity level', 'minimal')
  .parse();

const options = program.opts();

console.log(chalk.blue('🔨 Building Hangfire.WorkflowCore...'));
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

async function build() {
  try {
    // Restore packages
    await runCommand('dotnet', ['restore'], 'Restoring NuGet packages');
    
    // Build solution
    await runCommand('dotnet', [
      'build',
      '--configuration', options.configuration,
      '--verbosity', options.verbosity,
      '--no-restore'
    ], `Building solution (${options.configuration})`);

    console.log(chalk.green('\n🎉 Build completed successfully!'));
    
    // Show build outputs
    console.log(chalk.blue('\n📦 Build outputs:'));
    const { execSync } = require('child_process');
    try {
      const outputs = execSync(`find bin -name "*.dll" -o -name "*.exe" | head -10`, { encoding: 'utf8' });
      outputs.split('\n').filter(line => line.trim()).forEach(line => {
        console.log(chalk.gray(`  ${line}`));
      });
    } catch (e) {
      console.log(chalk.gray('  (Build outputs in bin/ directory)'));
    }

  } catch (error) {
    console.error(chalk.red('\n💥 Build failed!'));
    process.exit(1);
  }
}

build();