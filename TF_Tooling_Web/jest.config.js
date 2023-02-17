/** @type {import('ts-jest').JestConfigWithTsJest} */
module.exports = {
    preset: 'ts-jest',
    testEnvironment: 'jsdom',
    collectCoverageFrom: ['./src/**/*.ts'],
    setupFilesAfterEnv: ['jest-extended-snapshot'],
    coverageReporters: ['cobertura', 'text', 'text-summary', 'html']
};
