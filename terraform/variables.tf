variable "region" {
  description = "AWS region where resources will be created"
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Name of the project"
  type        = string
  default     = "imdb-for-music"
}

variable "frontend_project_path" {
  description = "Path to the frontend project directory"
  type        = string
  default     = "../frontend"
}

variable "build_and_upload_frontend" {
  description = "Whether to build and upload the frontend during apply"
  type        = bool
  default     = true
}

variable "force_destroy_s3" {
  description = "Whether to force destroy the S3 bucket even if it contains objects"
  type        = bool
  default     = true
}

variable "domain_name" {
  description = "Domain name for the application"
  type        = string
  default     = "academichub.net"
}

variable "use_acm_certificate" {
  description = "Whether to use a custom ACM certificate for CloudFront and ALB"
  type        = bool
  default     = true
}

variable "acm_certificate_arn" {
  description = "ARN of the ACM certificate to use for CloudFront and ALB (should be a wildcard certificate)"
  type        = string
  default     = null
  sensitive   = true
}

variable "create_api_records" {
  description = "Whether to create Route53 records for the API domain"
  type        = bool
  default     = true
}

variable "rds_instance_class" {
  type = map(string)
  default = {
    dev  = "db.t4g.micro"
    prod = "db.t4g.micro"
  }
}

variable "rds_multi_az" {
  description = "Whether to enable Multi-AZ deployment for RDS"
  type        = bool
  default     = false
}

variable "rds_deletion_protection" {
  description = "Whether to enable deletion protection for RDS"
  type        = bool
  default     = false
}

variable "rds_skip_final_snapshot" {
  description = "Whether to skip final snapshot when destroying RDS"
  type        = bool
  default     = true
}

variable "rds_database_name" {
  description = "The name of RDS PostgresSQL database"
  type        = string
}

variable "rds_username" {
  description = "The username of RDS PostgresSQL database"
  type        = string
}

variable "redis_node_type" {
  description = "Redis node type"
  type        = string
}

# MongoDB Atlas Credentials
variable "mongodb_atlas_org_id" {
  description = "MongoDB Atlas organization ID"
  type        = string
  sensitive   = true
}

variable "mongodb_atlas_public_key" {
  description = "MongoDB Atlas public API key"
  type        = string
  sensitive   = true
}

variable "mongodb_atlas_private_key" {
  description = "MongoDB Atlas private API key"
  type        = string
  sensitive   = true
}

# MongoDB Atlas Configuration
variable "mongodb_atlas_region" {
  description = "MongoDB Atlas region mapping by environment"
  type        = map(string)
  default = {
    dev  = "US_EAST_1"
    prod = "US_EAST_1"
  }
}

variable "mongodb_instance_size" {
  description = "MongoDB Atlas instance size mapping by environment"
  type        = map(string)
  default = {
    dev  = "M10"
    prod = "M10"
  }
}

variable "mongodb_disk_size_gb" {
  description = "MongoDB Atlas disk size in GB mapping by environment"
  type        = map(number)
  default = {
    dev  = 10
    prod = 10
  }
}

variable "mongodb_database_name" {
  description = "Name of the MongoDB database"
  type        = string
  sensitive   = true
  default     = "musicapp"
}

variable "mongodb_username" {
  description = "Username for MongoDB Atlas database user"
  type        = string
  sensitive   = true
  default     = "app_user"
}