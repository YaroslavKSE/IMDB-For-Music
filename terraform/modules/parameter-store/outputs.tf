output "parameter_arns" {
  description = "Map of parameter names to their ARNs"
  value = {
    for k, v in aws_ssm_parameter.parameter : k => v.arn
  }
}

output "parameter_names" {
  description = "Map of parameter names to their full SSM path names"
  value = {
    for k, v in aws_ssm_parameter.parameter : k => v.name
  }
}