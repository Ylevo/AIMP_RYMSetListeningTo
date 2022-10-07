# AIMP_RYMSetListeningTo
AIMP plugin for the new feature "Listening To" on rateyourmusic.com.

**You have to enter your RYM authentification token in the config.xml file included in the release for the plugin to work**. 
You can find it in your rateyourmusic.com cookies using the storage inspector of your browser. The cookie's name is "ulv".

RYM's server seems very sensitive to successive HTTP requests and can quickly block yours if you go too fast on them. Considering that, I've added a minimum of 5 seconds of listening to the current track before making any request. It's set to 15 secondes by default in the config file.

RYM search engine is far from perfect and will sometimes not return the correct album as first result, resulting in a bit wrong Set Listening To. I can't do anything about that. RYM makes it possible to precise what track you're currently listening to, but with all the different standards for track listing (see https://rateyourmusic.com/wiki/Music:Standards%20for%20track%20listings) while audio files usually use mere track numbers in metadata, it often fails to match. It is technically possible to request the album page and look for the track ID, but that would be yet another request to a quite sensitive server.
