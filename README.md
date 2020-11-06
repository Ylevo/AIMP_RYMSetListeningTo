# AIMP_SetListeningTo
AIMP plugin for the new feature "Listening To" on rateyourmusic.com.

**You have to enter your RYM authentification token in the config.xml file included in the release for the plugin to work**. 
You can find it in your rateyourmusic.com cookies using the storage inspector of your browser. The cookie's name is "ulv".

RYM's server seems very sensitive to successive HTTP requests and can quickly block yours if you go too fast on them. Considering that, I've added a minimum of 5 seconds of listening to the current track before making any request. It's set to 15 secondes by default in the config file.

Also, the code is dirty.
