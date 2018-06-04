module main;


import System.{AttributeUsageAttribute, AttributeTargets} as {AttributeUsageAttribute, AttributeTargets};

class SomeClass
{
    let x (- int;
}

#[AttributeUsage{validOn: AttributeTargets.Class}]
class SomeAttribute
{
    let x (- SomeClass;
}

#[Some{x: SomeClass{x: 10}}]
class AttributedClass
{
    let y (- int;
}