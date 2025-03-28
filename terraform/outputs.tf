output "vpc_id" {
  description = "The ID of the VPC"
  value       = module.vpc.vpc_id
}

output "public_subnet_id" {
  description = "The ID of the public subnet"
  value       = module.vpc.public_subnet_id
}

output "private_subnet_id" {
  description = "The ID of the private subnet"
  value       = module.vpc.private_subnet_id
}

output "mongodb_vpc_endpoint_id" {
  description = "The ID of the MongoDB Atlas VPC Endpoint (if created)"
  value       = module.vpc.mongodb_vpc_endpoint_id
}

output "mongodb_vpc_endpoint_created" {
  description = "Whether the MongoDB VPC endpoint was created"
  value       = local.config.create_mongodb_endpoint
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