#ifndef SC_NAVBAR_H
#define SC_NAVBAR_H

#include "common.h"

#include <stdbool.h>
#include <SDL3/SDL_events.h>
#include <SDL3/SDL_render.h>
#include <SDL3/SDL_video.h>

// Height of the navigation bar, in window points
#define SC_NAVBAR_HEIGHT 56

struct sc_navbar {
    int hover;   // index of the hovered button, or -1
    int pressed; // index of the pressed button, or -1
};

void
sc_navbar_init(struct sc_navbar *nb);

// Height of the navigation bar in window coordinates (DPI-aware)
float
sc_navbar_get_height(SDL_Window *window);

// Handle a mouse event. Return true if the event was consumed by the navbar.
// If a button was activated, *keycode is set to the Android keycode to inject
// (-1 otherwise).
bool
sc_navbar_handle_event(struct sc_navbar *nb, SDL_Window *window,
                       const SDL_Event *event, int *keycode);

// Draw the navigation bar over the bottom of the window
void
sc_navbar_render(struct sc_navbar *nb, SDL_Renderer *renderer,
                 SDL_Window *window);

#endif
