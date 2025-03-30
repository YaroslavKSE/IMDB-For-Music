variable "environment" {
  description = "Environment name (e.g., dev, prod)"
  type        = string
}

variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
}

variable "public_subnet_cidr_a" {
  description = "CIDR block for the public subnet in AZ a"
  type        = string
}

variable "public_subnet_cidr_b" {
  description = "CIDR block for the public subnet in AZ b"
  type        = string
}

variable "private_subnet_cidr_a" {
  description = "CIDR block for the private subnet in AZ a"
  type        = string
}

variable "private_subnet_cidr_b" {
  description = "CIDR block for the private subnet in AZ b"
  type        = string
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

variable "common_tags" {
  description = "Common tags to be applied to all resources"
  type        = map(string)
  default     = {}
}