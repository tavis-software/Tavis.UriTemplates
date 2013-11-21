# Uri Templates #

.Net implementation of the [URI Template Spec RFC6570](http://tools.ietf.org/html/rfc6570).

Library implements Level 4 compliance and is tested against test cases from [UriTemplate test suite](https://github.com/uri-templates/uritemplate-test).

Current this library does not pass the tests that use non-ascii range Unicode characters in a URL.  The spec calls for the
characters to use Unicode Normalization, which so far, I have not been able to reproduce in .net.

Also, many of the non-valid template tests do not yet pass. 