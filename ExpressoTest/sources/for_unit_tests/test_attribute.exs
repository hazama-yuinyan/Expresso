module test_attribute;


import System.{Attribute, AttributeUsageAttribute, AttributeTargets, ObsoleteAttribute} as {Attribute, AttributeUsageAttribute, AttributeTargets, ObsoleteAttribute};


#[AttributeUsage{validOn: AttributeTargets.All}]
export class AuthorAttribute : Attribute
{
	let name (- string;
}

#[Obsolete]
export def doSomethingInModule()
{
    ;
}

#[Author{name: "train12"}]
export let y = 100;