module test_attribute;


import System.{Attribute, AttributeUsageAttribute, AttributeTargets} as {Attribute, AttributeUsageAttribute, AttributeTargets};


#[AttributeUsage{validOn: AttributeTargets.All}]
export class AuthorAttribute : Attribute
{
	let name (- string;
}