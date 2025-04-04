# AWS Infrastructure Terraform Project

This project contains a complete Terraform configuration for deploying a web application infrastructure on AWS, including networking, content delivery, database services, and DNS management.

## Directory Structure

```
.
├── backend.tf              # Terraform backend configuration
├── main.tf                 # Main Terraform configuration file
├── variables.tf            # Root variables
├── outputs.tf              # Root outputs
├── environments/
│   ├── dev.tfvars          # Development environment variables
│   └── prod.tfvars         # Production environment variables
└── modules/
    ├── vpc/
    │   ├── main.tf         # VPC module resources
    │   ├── variables.tf    # VPC module variables
    │   └── outputs.tf      # VPC module outputs
    ├── s3/
    │   ├── main.tf         # S3 bucket and frontend deployment
    │   ├── variables.tf    # S3 module variables
    │   └── outputs.tf      # S3 module outputs
    ├── cloudfront/
    │   ├── main.tf         # CloudFront distribution and OAC
    │   ├── variables.tf    # CloudFront module variables
    │   └── outputs.tf      # CloudFront module outputs
    ├── route53/
    │   ├── main.tf         # Route53 DNS records
    │   ├── variables.tf    # Route53 module variables
    │   └── outputs.tf      # Route53 module outputs
    ├── alb/
    │   ├── main.tf         # Application Load Balancer
    │   ├── variables.tf    # ALB module variables
    │   └── outputs.tf      # ALB module outputs
    ├── rds/
    │   ├── main.tf         # PostgreSQL RDS database
    │   ├── variables.tf    # RDS module variables
    │   └── outputs.tf      # RDS module outputs
    ├── redis/
    │   ├── main.tf         # Redis ElastiCache
    │   ├── variables.tf    # Redis module variables
    │   └── outputs.tf      # Redis module outputs
    ├── mongodb/
    │   ├── main.tf         # MongoDB Atlas cluster and PrivateLink
    │   ├── variables.tf    # MongoDB module variables
    │   ├── outputs.tf      # MongoDB module outputs
    │   └── providers.tf    # MongoDB Atlas provider configuration
    ├── ecs/
    │   ├── main.tf         # ECS cluster and services
    │   ├── variables.tf    # ECS module variables
    │   ├── outputs.tf      # ECS module outputs
    │   └── tasks/          # Task definition modules
    │       ├── user_service/
    │       ├── music_catalog_service/
    │       └── music_interaction_service/
    └── parameter-store/
        ├── main.tf         # SSM Parameter Store resources
        ├── variables.tf    # Parameter Store module variables
        └── outputs.tf      # Parameter Store module outputs
```

## Infrastructure Components

The Terraform configuration creates the following infrastructure:

### Network Infrastructure (VPC Module)
- VPC with DNS support enabled
- Public subnets in us-east-1a and us-east-1b for high availability
- Private subnets in us-east-1a and us-east-1b for database and application layers
- Internet Gateway for public internet access
- NAT Gateway for private subnet outbound access
- Route tables for both public and private subnets

### Frontend Hosting (S3 Module)
- S3 bucket for static website files
- Security configuration to block public access
- Optional automated frontend build and deployment process
- Support for force destroy to enable clean infrastructure teardown

### Content Delivery (CloudFront Module)
- CloudFront distribution with Origin Access Control
- Price Class 100 (US, Canada, Europe)
- Optimal caching configuration
- SPA support with 404 handling
- Support for custom domain with HTTPS
- Cache invalidation on deployment

### Application Load Balancer (ALB Module)
- Application Load Balancer in public subnets
- HTTPS listener with certificate from ACM
- HTTP to HTTPS redirection
- Target groups for different backend services
- Path-based routing rules for API endpoints
- Security group with appropriate access controls

### PostgreSQL Database (RDS Module)
- PostgreSQL 17.2 database instance
- Free tier eligible configuration for development
- Configurable Multi-AZ deployment for high availability
- Parameter group optimized for application workloads
- Automated backups with configurable retention periods
- Password securely stored in AWS SSM Parameter Store
- Option for storage encryption and deletion protection

### Redis Cache (Redis Module)
- Redis 6.2 ElastiCache cluster
- Efficient node type selection (cache.t3.micro for development)
- Subnet group in private subnets for security
- Parameter group with LRU eviction policy
- Connection information stored in SSM Parameter Store
- Security group with controlled access from application services

### MongoDB Atlas (MongoDB Module)
- MongoDB Atlas project and MongoDB 8.0 cluster
- M10+ instance tier with auto-scaling support
- AWS PrivateLink integration for secure connectivity
- VPC endpoint in private subnets
- Database user creation and permissions
- Credentials securely stored in AWS SSM Parameter Store
- Security group for controlled access to MongoDB endpoint

### Containers and Orchestration (ECS Module)
- ECS Fargate cluster for containerized microservices
- Task definitions for multiple microservices (User Service, Music Catalog Service, Music Interaction Service)
- Auto-scaling configurations based on CPU and memory utilization
- IAM roles and policies for ECS task execution
- CloudWatch log groups for centralized logging
- Service integrations with ALB target groups
- Security groups for controlled network access
- Container environment variables and secrets from SSM Parameter Store

### Parameter Management (Parameter Store Module)
- Centralized management of application secrets and configuration
- Secure storage of sensitive credentials (Auth0, Spotify API)
- Consistent parameter naming convention with environment prefixes
- Support for various parameter types (SecureString, String)
- Integration with ECS task definitions for secure access

### DNS Management (Route53 Module)
- A and AAAA records pointing to CloudFront for frontend
- A and AAAA records pointing to ALB for API endpoints
- Support for both root domain and www subdomain
- API subdomain configuration for environment-specific endpoints

## Usage Instructions

### Initialize Terraform

```bash
terraform init
```

### Select Workspace (Environment)

For development environment:
```bash
terraform workspace new dev
# or select if it already exists
terraform workspace select dev
```

For production environment:
```bash
terraform workspace new prod
# or select if it already exists
terraform workspace select prod
```

### Plan and Apply

```bash
terraform plan -var-file="environments/dev.tfvars"
terraform apply -var-file="environments/dev.tfvars"
```

### Destroy Infrastructure

To destroy all resources:

```bash
terraform destroy -var-file="environments/dev.tfvars"
```

Note: The S3 bucket will be automatically emptied before deletion if `force_destroy_s3` is set to `true` (default).

## Configuration Options

### Environment-Specific Configuration

Key settings that differ between environments:

| Setting | Development | Production |
|---------|-------------|------------|
| VPC CIDR | 10.0.0.0/16 | 10.1.0.0/16 |
| RDS Instance | db.t4g.micro | db.t4g.small |
| RDS Multi-AZ | false | true |
| RDS Deletion Protection | false | true |
| RDS Skip Final Snapshot | true | false |
| Redis Node Type | cache.t3.micro | cache.t3.small |
| MongoDB Instance | M10 | M30 |
| MongoDB Disk Size | 10 GB | 30 GB |
| Domain | dev.academichub.net | academichub.net |
| API Subdomain | api-dev | api |

### PostgreSQL RDS Configuration

You can configure the RDS PostgreSQL database through variables:

```bash
# Set custom database name
terraform apply -var-file="environments/dev.tfvars" -var="rds_database_name=customdb"

# Configure RDS Multi-AZ deployment
terraform apply -var-file="environments/dev.tfvars" -var="rds_multi_az=true"

# Configure deletion protection
terraform apply -var-file="environments/dev.tfvars" -var="rds_deletion_protection=true"
```

### Redis Cache Configuration

You can configure the Redis ElastiCache through variables:

```bash
# Set custom Redis node type
terraform apply -var-file="environments/dev.tfvars" -var="redis_node_type=cache.t3.small"
```

### MongoDB Atlas Configuration

You can configure the MongoDB Atlas cluster through variables:

```bash
# Set MongoDB instance size
terraform apply -var-file="environments/dev.tfvars" -var="mongodb_instance_size={\"dev\"=\"M10\", \"prod\"=\"M30\"}"

# Set MongoDB disk size
terraform apply -var-file="environments/dev.tfvars" -var="mongodb_disk_size_gb={\"dev\"=10, \"prod\"=30}"
```

### ECS Services Configuration

You can configure the ECS services through variables:

```bash
# Set desired task count for user service
terraform apply -var-file="environments/dev.tfvars" -var="user_service_desired_count=2"

# Set CPU and memory limits for music catalog service
terraform apply -var-file="environments/dev.tfvars" -var="music_catalog_service_cpu=512" -var="music_catalog_service_memory=1024"

# Configure auto-scaling limits for music interaction service
terraform apply -var-file="environments/dev.tfvars" -var="music_interaction_service_min_capacity=2" -var="music_interaction_service_max_capacity=5"
```

### Parameter Store Configuration

Sensitive parameters are stored securely in Parameter Store:

```bash
# Update Auth0 configuration
terraform apply -var-file="environments/dev.tfvars" -var="auth0_domain=yourdomain.auth0.com" -var="auth0_client_id=yourclientid"

# Update Spotify API credentials
terraform apply -var-file="environments/dev.tfvars" -var="spotify_client_id=yourspotifyid" -var="spotify_client_secret=yoursecret"
```

### Frontend Deployment

You can control the frontend build and upload process:

```bash
# Skip frontend build and upload (useful for CI/CD pipelines)
terraform apply -var-file="environments/dev.tfvars" -var="build_and_upload_frontend=false"

# Include frontend build and upload (default)
terraform apply -var-file="environments/dev.tfvars" -var="build_and_upload_frontend=true"
```

### S3 Bucket Cleanup

Control whether the S3 bucket should be automatically emptied before deletion:

```bash
# Enable auto-cleanup of S3 bucket on destroy (default)
terraform apply -var-file="environments/dev.tfvars" -var="force_destroy_s3=true"

# Disable auto-cleanup (requires manual emptying before destroy)
terraform apply -var-file="environments/dev.tfvars" -var="force_destroy_s3=false"
```

## Accessing Database Resources

### From ECS Services

ECS task definitions are configured to access database services through environment variables and secrets:

```json
"secrets": [
  {
    "name": "ConnectionStrings__DefaultConnection",
    "valueFrom": "arn:aws:ssm:[region]:[account-id]:parameter/[environment]/database/connection_string"
  },
  {
    "name": "ConnectionStrings__Redis",
    "valueFrom": "arn:aws:ssm:[region]:[account-id]:parameter/[environment]/redis/connection_string"
  },
  {
    "name": "MongoDB__ConnectionString",
    "valueFrom": "arn:aws:ssm:[region]:[account-id]:parameter/[environment]/mongodb/[db-name]/connection_string"
  }
],
"environment": [
  {
    "name": "ASPNETCORE_ENVIRONMENT",
    "value": "Production"
  },
  {
    "name": "AllowedOrigins",
    "value": "https://academichub.net,https://www.academichub.net"
  }
]
```

## Microservice Architecture

The application is built using a microservice architecture with three main services:

### User Service
- Handles user authentication and profile management
- Uses PostgreSQL for persistent data storage
- Integrates with Auth0 for identity management
- Exposed at `/api/v1/auth/*` and `/api/v1/users/*` endpoints

### Music Catalog Service
- Manages music metadata and catalog information
- Uses MongoDB for flexible schema storage
- Uses Redis for caching frequently accessed data
- Integrates with Spotify API for additional music data
- Exposed at `/api/v1/catalog/*` endpoints

### Music Interaction Service
- Handles user interactions with music (ratings, reviews, etc.)
- Uses a dedicated PostgreSQL database
- Integrates with MongoDB for complex data queries
- Exposed at `/api/v1/rating/*`, `/api/grading-methods/*`, and `/api/interactions/*` endpoints

## Important Notes

1. The S3 backend configuration in `backend.tf` requires a DynamoDB table named `terraform-locks` with a partition key named "LockID" of type String.

2. Environment-specific CIDR blocks are defined in the main.tf file:
   - Development: VPC CIDR `10.0.0.0/16`, Public Subnet `10.0.1.0/24`, Private Subnet `10.0.3.0/24`
   - Production: VPC CIDR `10.1.0.0/16`, Public Subnet `10.1.1.0/24`, Private Subnet `10.1.3.0/24`

3. The MongoDB Atlas module requires:
   - A MongoDB Atlas account and organization
   - API keys with appropriate permissions
   - The MongoDB Atlas provider will be automatically installed during terraform init

4. All database services (RDS, Redis, MongoDB) are deployed in private subnets for security.

5. The frontend project path is set to `../frontend` by default, relative to your Terraform directory.

6. If using a custom domain with HTTPS:
   - Set `use_acm_certificate = true` in your environment variables
   - Provide an ACM certificate ARN through `acm_certificate_arn` variable
   - Ensure the SSL certificate is created in the `us-east-1` region for CloudFront

7. Route53 configuration assumes you've already created a public hosted zone for your domain.

8. Security groups are configured to allow traffic only from the appropriate sources.

9. Passwords and connection strings are stored in AWS SSM Parameter Store for secure access.

10. ECS tasks require IAM permissions to access the SSM Parameter Store.

11. ECR repositories for container images must be created before applying the Terraform configuration.

12. The ECS module uses Fargate launch type for all services, eliminating the need to manage EC2 instances.

13. Auto-scaling is configured for all ECS services based on CPU and memory utilization metrics.

## Free Tier and Cost Considerations

### PostgreSQL RDS
- Instance Type: `db.t4g.micro` is free tier eligible
- Storage: 20 GB included in free tier
- Storage Encryption: Disabled in dev to remain free tier eligible
- Multi-AZ: Disabled in dev (not free tier eligible)

### Redis ElastiCache
- Instance Type: `cache.t3.micro` is the smallest available option
- Note: ElastiCache is not strictly part of the AWS free tier, but `cache.t3.micro` is the most cost-effective option

### MongoDB Atlas
- Instance Type: M10 is the smallest tier that supports AWS PrivateLink
- M10 provides 2 vCPUs, 2 GB RAM, and 10 GB storage
- Note: MongoDB Atlas has its own pricing model and is not part of AWS free tier
- The MongoDB Atlas module is configured to use auto-scaling to optimize costs

### ECS Fargate
- Smaller task sizes (256 CPU units, 512 MB memory) are used in dev environment
- Auto-scaling with appropriate minimum and maximum service values
- CloudWatch Container Insights enabled for enhanced monitoring

### Application Load Balancer
- One ALB is shared across all microservices to reduce costs
- Path-based routing is used to direct traffic to appropriate services

## Key Variables

Key variables to set in your environment files:

```hcl
# Region and Project
region = "us-east-1"
project_name = "music-catalog-service"

# Frontend Configuration
frontend_project_path = "../frontend"
build_and_upload_frontend = true
force_destroy_s3 = true

# Domain and Certificate
domain_name = "academichub.net"
use_acm_certificate = true
acm_certificate_arn = "arn:aws:acm:us-east-1:YOUR_ACCOUNT_ID:certificate/YOUR_CERTIFICATE_ID"

# RDS Configuration
rds_database_name = "musicapp"
rds_username = "dbadmin"
rds_multi_az = false  # true for production
rds_deletion_protection = false  # true for production
rds_skip_final_snapshot = true  # false for production

# Redis Configuration
redis_node_type = "cache.t3.micro"  # larger for production

# MongoDB Atlas Configuration
mongodb_database_name = "musicapp"
mongodb_username = "app_user"
mongodb_instance_size = {
  dev  = "M10"
  prod = "M30"
}
mongodb_disk_size_gb = {
  dev  = 10
  prod = 30
}

# ECR Repository URLs
user_service_repository_url = "123456789012.dkr.ecr.us-east-1.amazonaws.com/user-service"
music_catalog_service_repository_url = "123456789012.dkr.ecr.us-east-1.amazonaws.com/music-catalog-service"
music_interaction_service_repository_url = "123456789012.dkr.ecr.us-east-1.amazonaws.com/music-interaction-service"

# ECS Configuration
user_service_cpu = 256
user_service_memory = 512
user_service_desired_count = 1
user_service_min_capacity = 1
user_service_max_capacity = 3

music_catalog_service_cpu = 256
music_catalog_service_memory = 512
music_catalog_service_desired_count = 1
music_catalog_service_min_capacity = 1
music_catalog_service_max_capacity = 3

music_interaction_service_cpu = 256
music_interaction_service_memory = 512
music_interaction_service_desired_count = 1
music_interaction_service_min_capacity = 1
music_interaction_service_max_capacity = 3

# Auth0 Configuration
auth0_domain = "your-tenant.auth0.com"
auth0_client_id = "your-client-id"
auth0_client_secret = "your-client-secret"
auth0_audience = "your-api-audience"
auth0_management_api_audience = "https://your-tenant.auth0.com/api/v2/"

# Spotify API Configuration
spotify_client_id = "your-spotify-client-id"
spotify_client_secret = "your-spotify-client-secret"
```

## Security Best Practices

1. **MongoDB Atlas Credentials**: Store these in a `secrets.auto.tfvars` file or environment variables and never commit them to source control:
   ```
   mongodb_atlas_org_id = "your-organization-id"
   mongodb_atlas_public_key = "your-public-key"
   mongodb_atlas_private_key = "your-private-key"
   ```

2. **AWS VPC PrivateLink**: MongoDB Atlas is connected to your VPC using AWS PrivateLink, which provides secure private connectivity without exposing traffic to the public internet.

3. **Security Groups**: All database services have security groups that restrict access to only necessary sources.

4. **SSM Parameter Store**: All credentials are stored securely in AWS Systems Manager Parameter Store.

5. **ECS Task Execution Role**: Principle of least privilege is applied to the ECS task execution role, allowing only the minimum permissions required.

6. **HTTPS Enforcement**: All traffic is encrypted with HTTPS, and HTTP is automatically redirected to HTTPS.

7. **Private Subnets**: All application and database resources are deployed in private subnets with no direct internet access.

## Next Steps

After successful deployment, you should:

1. Verify CloudFront distribution is properly connected to your S3 bucket
2. Confirm DNS resolution is working correctly for your domain
3. Test your website through both the CloudFront URL and your custom domain
4. Verify the API endpoints are accessible through the ALB
5. Configure your application to use the RDS, Redis, and MongoDB services
6. Implement a CI/CD pipeline for automated deployments
7. Set up monitoring and alerting for all resources
8. Configure WAF and Shield for enhanced security (especially in production)
9. Implement additional observability tools for comprehensive application monitoring

## Troubleshooting

### CloudFront Invalidation Issues

If you encounter issues with CloudFront invalidation during deployment:

1. Check that AWS CLI is properly installed and configured with appropriate permissions
2. Verify that the invalidation path pattern is correctly formatted (`/*`)
3. If using Windows, ensure command line arguments are properly escaped

### S3 Bucket Deletion Problems

If you have trouble destroying the S3 bucket:

1. Check if `force_destroy_s3` is set to `true`
2. If set to `false`, manually empty the bucket before running `terraform destroy`:
   ```bash
   aws s3 rm s3://your-bucket-name --recursive
   ```
3. Wait a few minutes after emptying the bucket before attempting to destroy it

### Database Connection Issues

If your application cannot connect to the database:

1. Verify the security group rules allow traffic from your application
2. Check that your application has the correct IAM permissions to read from SSM Parameter Store
3. Ensure the database parameters (host, port, username, password) are correctly passed to your application
4. For MongoDB Atlas, verify the AWS PrivateLink connection is established correctly

### MongoDB Atlas Issues

If you encounter issues with MongoDB Atlas:

1. Verify your Atlas organization ID and API keys are correct
2. Check that the MongoDB Atlas provider is correctly installed
3. Ensure your AWS VPC has connectivity to the MongoDB Atlas PrivateLink endpoint
4. Verify that the security group allows traffic on port 27017 from your application

### ECS Service Deployment Failures

If ECS services fail to deploy properly:

1. Check CloudWatch Logs for task startup errors
2. Verify that the container image exists in the specified ECR repository
3. Ensure the ECS task execution role has permission to pull images and get parameters
4. Validate the health check configuration and make sure the endpoint is responding correctly
5. Check if the task is exceeding CPU or memory limits during startup

### Parameter Store Access Issues

If ECS tasks cannot access parameters:

1. Verify that the ECS task execution role has the appropriate permissions
2. Check that the parameter ARNs in the task definition are correct
3. Ensure parameters exist in the correct region and with the correct names
4. Validate that the parameter type (String, SecureString) is correctly configured