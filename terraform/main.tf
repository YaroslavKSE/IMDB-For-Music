provider "aws" {
  region = var.region
}

locals {
  environment = terraform.workspace

  # Environment-specific configurations
  env_configs = {
    dev = {
      vpc_cidr                = "10.0.0.0/16"
      public_subnet_cidr_a    = "10.0.1.0/24"
      public_subnet_cidr_b    = "10.0.2.0/24"
      private_subnet_cidr_a   = "10.0.3.0/24"
      private_subnet_cidr_b   = "10.0.4.0/24"
      frontend_bucket         = "${var.project_name}-frontend-${terraform.workspace}"
      create_mongodb_endpoint = var.create_mongodb_endpoint
      api_subdomain           = "api-dev"
    }

    prod = {
      vpc_cidr                = "10.1.0.0/16"
      public_subnet_cidr_a    = "10.1.1.0/24"
      public_subnet_cidr_b    = "10.1.2.0/24"
      private_subnet_cidr_a   = "10.1.3.0/24"
      private_subnet_cidr_b   = "10.1.4.0/24"
      frontend_bucket         = "${var.project_name}-frontend-${terraform.workspace}"
      create_mongodb_endpoint = var.create_mongodb_endpoint
      api_subdomain           = "api"
    }
  }

  # Make sure we have a valid environment
  config = contains(keys(local.env_configs), local.environment) ? local.env_configs[local.environment] : local.env_configs["dev"]

  # Common tags for all resources
  common_tags = {
    Environment = local.environment
    ManagedBy   = "Terraform"
    Project     = var.project_name
  }
}

module "vpc" {
  source = "./modules/vpc"

  environment             = local.environment
  region                  = var.region
  vpc_cidr                = local.config.vpc_cidr
  public_subnet_cidr_a    = local.config.public_subnet_cidr_a
  public_subnet_cidr_b    = local.config.public_subnet_cidr_b
  private_subnet_cidr_a   = local.config.private_subnet_cidr_a
  private_subnet_cidr_b   = local.config.private_subnet_cidr_b
  create_mongodb_endpoint = local.config.create_mongodb_endpoint
  mongodb_service_name    = var.mongodb_service_name
  common_tags             = local.common_tags
}

module "s3" {
  source = "./modules/s3"

  environment               = local.environment
  bucket_name               = local.config.frontend_bucket
  frontend_project_path     = var.frontend_project_path
  build_and_upload_frontend = var.build_and_upload_frontend
  force_destroy             = var.force_destroy_s3
  common_tags               = local.common_tags
}

module "cloudfront" {
  source = "./modules/cloudfront"

  environment                 = local.environment
  bucket_id                   = module.s3.bucket_id
  bucket_arn                  = module.s3.bucket_arn
  bucket_regional_domain_name = module.s3.bucket_regional_domain_name
  domain_aliases              = var.use_acm_certificate ? [var.domain_name, "www.${var.domain_name}"] : []
  use_default_certificate     = !var.use_acm_certificate
  acm_certificate_arn         = var.acm_certificate_arn
  common_tags                 = local.common_tags

  depends_on = [module.s3]
}

# ALB module for API services
module "alb" {
  source = "./modules/alb"

  environment       = local.environment
  vpc_id            = module.vpc.vpc_id
  public_subnet_ids = module.vpc.public_subnet_ids # Using both public subnets for HA
  certificate_arn   = var.acm_certificate_arn      # Using the same wildcard certificate
  name_prefix       = "api"
  common_tags       = local.common_tags

  depends_on = [module.vpc]
}

module "route53" {
  source = "./modules/route53"

  domain_name               = var.domain_name
  cloudfront_domain_name    = module.cloudfront.cloudfront_domain_name
  cloudfront_hosted_zone_id = module.cloudfront.cloudfront_hosted_zone_id
  enable_ipv6               = false
  create_www_subdomain      = true
  create_api_records        = true
  api_subdomain             = local.config.api_subdomain
  alb_domain_name           = module.alb.alb_dns_name
  alb_hosted_zone_id        = module.alb.alb_zone_id
  common_tags               = local.common_tags

  depends_on = [module.cloudfront, module.alb]
}