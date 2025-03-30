output "vpc_id" {
  description = "The ID of the VPC"
  value       = module.vpc.vpc_id
}

output "environment" {
  description = "The current environment"
  value       = local.environment
}

output "frontend_bucket_name" {
  description = "The name of the S3 bucket for frontend files"
  value       = module.s3.bucket_id
}

output "cloudfront_domain_name" {
  description = "The CloudFront distribution domain name"
  value       = module.cloudfront.cloudfront_domain_name
}

output "cloudfront_id" {
  description = "The ID of the CloudFront distribution"
  value       = module.cloudfront.cloudfront_id
}

output "website_url" {
  description = "The URL of the website with CloudFront"
  value       = "https://${module.cloudfront.cloudfront_domain_name}"
}

output "domain_website_url" {
  description = "The URL of the website with custom domain"
  value       = "https://${var.domain_name}"
}

output "www_website_url" {
  description = "The URL of the website with www subdomain"
  value       = "https://www.${var.domain_name}"
}

# MongoDB Atlas Outputs
output "mongodb_project_id" {
  description = "MongoDB Atlas project ID"
  value       = module.mongodb.project_id
}

output "mongodb_cluster_name" {
  description = "MongoDB Atlas cluster name"
  value       = module.mongodb.cluster_name
}

output "mongodb_connection_string_param" {
  description = "SSM Parameter Store key for MongoDB Atlas connection string"
  value       = module.mongodb.mongodb_connection_string_parameter
}

output "mongodb_password_param" {
  description = "SSM Parameter Store key for MongoDB Atlas password"
  value       = module.mongodb.db_password_parameter
}

output "mongodb_privatelink_endpoint_id" {
  description = "MongoDB Atlas PrivateLink Endpoint ID"
  value       = module.mongodb.privatelink_endpoint_id
}

output "mongodb_vpc_endpoint_id" {
  description = "AWS VPC Endpoint ID for MongoDB Atlas"
  value       = module.mongodb.vpc_endpoint_id
}

output "mongodb_security_group_id" {
  description = "Security Group ID for MongoDB Atlas VPC Endpoint"
  value       = module.mongodb.security_group_id
}