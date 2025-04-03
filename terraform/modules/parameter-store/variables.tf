variable "environment" {
  description = "Environment name (e.g., dev, prod)"
  type        = string
}

variable "parameters" {
  description = "Map of parameter paths to their non-sensitive configuration"
  type = map(object({
    description = optional(string)
    type        = optional(string)
    tier        = optional(string)
  }))
  default = {}
}

variable "parameter_values" {
  description = "Map of parameter paths to their sensitive values"
  type        = map(string)
  sensitive   = true
}

variable "common_tags" {
  description = "Common tags to be applied to all resources"
  type        = map(string)
  default     = {}
}