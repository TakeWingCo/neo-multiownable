# neo-multiownable
[![Build Status](https://dev.azure.com/soloway2010/TakeWing.Neo.Multiownable/_apis/build/status/soloway2010.neo-multiownable?branchName=develop)](https://dev.azure.com/soloway2010/TakeWing.Neo.Multiownable/_build/latest?definitionId=1?branchName=develop)

Multiownable library for NEO Smart Contracts

## TakeWing.Neo.Multiownable
**Library for easy multisig and consensus.**

Multiownable API:

|						**API**						|											**Description**												|
| ------------------------------------------------- | ------------------------------------------------------------------------------------------------------|
| TakeWing.Neo.Multiownable.Sha256					| Call Sha256 by OpCode.																				|
| TakeWing.Neo.Multiownable.GetNumberOfOwners		| Get number of owners.																					|
| TakeWing.Neo.Multiownable.GetOwnerByIndex			| Get owner by his index.																				|
| TakeWing.Neo.Multiownable.GetIndexByOwner			| Get index of owner.																					|
| TakeWing.Neo.Multiownable.GetGenerationOfOwners	| Get current generation of owners number.																|
| TakeWing.Neo.Multiownable.GetAllOwners			| Get array of owners.																					|
| TakeWing.Neo.Multiownable.IsOwner					| Check if public key in owners list.																	|
| TakeWing.Neo.Multiownable.TransferOwnership		| Transfer ownership to new owners list.																|
| TakeWing.Neo.Multiownable.IsAcceptedBySomeOwners	| Check, that timeout doesn't expire and minimal required number of owners accepts call of function.	|

