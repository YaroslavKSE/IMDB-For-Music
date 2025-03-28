# AWS Infrastructure Terraform Project

This project contains a complete Terraform configuration for deploying a web application infrastructure on AWS, including networking, content delivery, and DNS management.

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
    └── route53/
        ├── main.tf         # Route53 DNS records
        ├── variables.tf    # Route53 module variables
        └── outputs.tf      # Route53 module outputs
```

## Infrastructure Components

The Terraform configuration creates the following infrastructure:

### Network Infrastructure (VPC Module)
- VPC with DNS support enabled
- One public subnet in us-east-1a
- One private subnet in us-east-1a
- Internet Gateway for public internet access
- NAT Gateway for private subnet outbound access
- Route tables for both subnets
- Optional VPC Endpoint for MongoDB Atlas (can be enabled/disabled)

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

### DNS Management (Route53 Module)
- A and AAAA records pointing to CloudFront
- Support for both root domain and www subdomain

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

### MongoDB VPC Endpoint

You can choose whether to create a VPC endpoint for MongoDB Atlas:

```bash
# To skip MongoDB VPC endpoint creation
terraform apply -var-file="environments/dev.tfvars" -var="create_mongodb_endpoint=false"

# To create MongoDB VPC endpoint (default)
terraform apply -var-file="environments/dev.tfvars" -var="create_mongodb_endpoint=true"
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

## Important Notes

1. The MongoDB Atlas VPC Endpoint service name may need to be updated with the actual service name.

2. The S3 backend configuration in `backend.tf` requires a DynamoDB table named `terraform-locks` with a partition key named "LockID" of type String.

3. Environment-specific CIDR blocks are defined in the main.tf file:
   - Development: VPC CIDR `10.0.0.0/16`, Public Subnet `10.0.1.0/24`, Private Subnet `10.0.2.0/24`
   - Production: VPC CIDR `10.1.0.0/16`, Public Subnet `10.1.1.0/24`, Private Subnet `10.1.2.0/24`

4. For high availability in production, consider adding resources in multiple availability zones.

5. The frontend project path is set to `../frontend` by default, relative to your Terraform directory.

6. If using a custom domain with HTTPS:
   - Set `use_acm_certificate = true` in your environment variables
   - Provide an ACM certificate ARN through `acm_certificate_arn` variable
   - Ensure the SSL certificate is created in the `us-east-1` region for CloudFront

7. Route53 configuration assumes you've already created a public hosted zone for your domain.

8. DynamoDB table for Terraform state locking must use "LockID" as the partition key.

## Key Variables

Key variables to set in your environment files:

```hcl
region = "us-east-1"
project_name = "music-catalog-service"
create_mongodb_endpoint = true
mongodb_service_name = "com.amazonaws.vpce.us-east-1.vpce-svc-mongodb"
frontend_project_path = "../frontend"
build_and_upload_frontend = true
force_destroy_s3 = true
domain_name = "academichub.net"
use_acm_certificate = true
acm_certificate_arn = "arn:aws:acm:us-east-1:YOUR_ACCOUNT_ID:certificate/YOUR_CERTIFICATE_ID"
```

## Next Steps

After successful deployment, you should:

1. Verify CloudFront distribution is properly connected to your S3 bucket
2. Confirm DNS resolution is working correctly for your domain
3. Test your website through both the CloudFront URL and your custom domain
4. Implement a CI/CD pipeline for automated deployments

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