variable "region" {
  description = "AWS region where resources will be created"
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Name of the project"
  type        = string
  default     = "my-project"
}

variable "create_mongodb_endpoint" {
  description = "Whether to create MongoDB Atlas VPC Endpoint"
  type        = bool
  default     = true
}

variable "mongodb_service_name" {
  description = "Service name for the MongoDB Atlas VPC Endpoint"
  type        = string
  default     = "com.amazonaws.vpce.us-east-1.vpce-svc-mongodb"
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
  description = "Whether to use a custom ACM certificate for CloudFront"
  type        = bool
  default     = true
}

variable "acm_certificate_arn" {
  description = "ARN of the ACM certificate to use for CloudFront"
  type        = string
  default     = null
}