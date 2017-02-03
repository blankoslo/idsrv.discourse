# IdentityServer integration w/ Discourse
This sample extends IdentityServer3 with endpoints enabling SSO with a Discourse instance.

# Why?
Discourse supports direct integration with Twitter, Yahoo, Google, but not a generic OpenId Connect Provider like IdentityServer3.

# How?

1. Enable SSO in Discourse
2. Create custom endpoints integrating with IdentityServer3

### 1. How to setup Discourse for custom SSO:
Go to your discourse instance as an admin at `/admin/site_settings/category/login`, and enable SSO:

![/images/discourse_sso_setup.PNG](/images/discourse_sso_setup.PNG)

For more information, [see Discourse own docs](https://meta.discourse.org/t/official-single-sign-on-for-discourse/13045) about this.


### 2. See [DiscourseController.cs](/Idsrv.Discourse/DiscourseController.cs)
These endpoints handle the communication between Discourse and IdentityServer3.
Discourse initiates a login session by providing a payload that can be validated by using the secret and SHA256.

By using running in the idsrv pipeline, `/core`, we can use IdentityServer extension methods on the current context. 
Here, we're using the  `GetIdentityServerFullLoginAsync()` extension. If idsrv says there is an authenticated user, we redirect back
to Discource with the custom response it needs to login a user in Discourse. Otherwise, we show a login form.

Links:
* [Discourse test instance in Azure](http://discourse-test.westeurope.cloudapp.azure.com/)

