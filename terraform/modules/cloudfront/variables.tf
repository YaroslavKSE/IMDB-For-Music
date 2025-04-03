variable "environment" {
  description = "Environment name (e.g., dev, prod)"
  type        = string
}

variable "bucket_id" {
  description = "ID of the S3 bucket for frontend files"
  type        = string
}

variable "bucket_arn" {
  description = "ARN of the S3 bucket for frontend files"
  type        = string
}

variable "bucket_regional_domain_name" {
  description = "Regional domain name of the S3 bucket"
  type        = string
}

variable "common_tags" {
  description = "Common tags to be applied to all resources"
  type        = map(string)
  default     = {}
}

variable "domain_aliases" {
  description = "List of alternate domain names for the CloudFront distribution"
  type        = list(string)
  default     = []
}

variable "use_default_certificate" {
  description = "Whether to use the default CloudFront certificate or an ACM certificate"
  type        = bool
  default     = true
}

variable "acm_certificate_arn" {
  description = "ARN of the ACM certificate to use for CloudFront"
  type        = string
  default     = null
}