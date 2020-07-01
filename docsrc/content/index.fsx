(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
FSecurity
======================

**FSecurity** is a testing library to simplifying Security Tests for .NET programs.
Splitted in three different packages, the security library provides additional functionality for specific area's.

 * `FSecurity`: is the base library and contain fuzzing logic to generate random inputs directly as `string` values.
 * `FSecurity.Api`: is the library that contains specific scanning functionality, fuzzing HTTP requests, and the ability to collect vulnerabilities.
 * `FSecurity.FsCheck`: is the library that contains `Gen<'a>` generators (See: [FsCheck](https://fscheck.github.io/FsCheck/index.html).

    Examples of exposed generators are:
    * `FSec.xssInject` returns a generator that for all different kinds of XSS payloads
    * `FSec.xpathInject` returns a generator for all different kinds of XPath injection
    * `FSec.xmlBomb` returns a generator that makes all different kinds of XML payload that uses the XRE (External Reference Entity) functionality to exploit missue of XML parsers.

Samples & documentation
-----------------------

 * [API testing](reference/api.html) explains how application security testing can be done against API services.
 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding a new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read the [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/stijnmoreels/FSec/tree/master/docs/content
  [gh]: https://github.com/stijnmoreels/FSec
  [issues]: https://github.com/stijnmoreels/FSec/issues
  [readme]: https://github.com/stijnmoreels/FSec/blob/master/README.md
  [license]: https://github.com/stijnmoreels/FSec/blob/master/LICENSE.txt
*)
