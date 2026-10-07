# How the pipiline works

## CI/CD Workflows

This project uses four GitHub Actions workflows to drive its CI/CD pipeline:

1. check.yml
2. deploy_to_test.yml
3. deploy_to_prod.yml
4. deploy_latest.yml

**check.yml**

This runs on every pull request. The merging is blocked until all checks pass, which keeps bad code, failing tests, and broken builds out of the main branch.

**deploy_to_test.yml and deploy_to_prod.yml**

These workflows build the application and deploy it to their target environment whenever a release is published:
* deploy_to_test.yml runs when a release is published as a pre-release and deploys to the test environment.
* deploy_to_prod.yml runs when a release is published as a full release and deploys to the production environment.

**deploy_latest.yml**

Provides a rollback path in case a broken build is deployed by mistake. It runs when a previous release is marked as latest, redeploying that release.